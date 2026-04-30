# Bridge — API 設計

## 1. 設計方針

### 1.1 スタイル
- **緩めのREST**(リソース指向を基本に、アクション系エンドポイントを許容)
- **JSON** レスポンス(成功・エラーともに構造化)
- **camelCase** フィールド名(.NET Core の `System.Text.Json` デフォルトに合わせる)

### 1.2 守るルール
- リソース指向のURL(`/projects`, `/engineers`, `/assignments`)
- HTTPメソッドの意味を守る(GET/POST/PATCH/DELETE)
- 適切なステータスコード(200/201/204/400/401/403/404/409/500)
- エラーレスポンスは構造化(後述)

### 1.3 緩める部分
- アクション系エンドポイント許容(`POST /auth/login`、`POST /assignments/{id}/contracts` 等)
- ネスト深さは 2 階層まで
- 検索・フィルタはクエリパラメータで表現
- 必要に応じて関連リソースの埋め込みを許容
- リソース所有者起点のサブパス許容(`/engineers/me`、`/sales/me/expiring-contracts` 等)

---

## 2. 認証方式

### 2.1 方針:JWT(アクセストークンのみ)

**理由**:SPA(Vercel)と API(Fly.io)を**別ドメインにデプロイするクロスオリジン構成**のため、セッションCookieより JWT が素直に扱える。MVP ではリフレッシュトークンを省略し、アクセストークン 1本構成で実装コストを抑える。

### 2.2 トークン仕様
- **アルゴリズム**:HS256(対称鍵)
- **有効期限**:24時間
- **保管場所**:フロント側の React state(メモリ)のみ
- **注意**:ブラウザリロードで失われるため、リロード時は再ログインが必要。UI側で再ログイン誘導を実装する
- **ペイロード**:
```json
{
  "sub": "7",            // UserId
  "role": "Engineer",    // Sales | Engineer | Admin
  "engineerId": 7,       // Engineer ロール時のみ
  "salesId": null,       // Sales ロール時のみ
  "exp": 1234567890
}
```

### 2.3 エンドポイント動作

**`POST /auth/login`** リクエスト:
```json
{ "email": "tanaka@example.com", "password": "..." }
```
レスポンス:
```json
{
  "token": "eyJhbGc...",
  "user": {
    "id": 7,
    "email": "tanaka@example.com",
    "role": "Engineer"
  }
}
```

**`POST /auth/logout`** サーバー側は実質 no-op(ステートレスのため)。フロント側でトークンを破棄する。

**`GET /auth/me`** 現在の認証ユーザーの基本情報を返す。

### 2.4 フロント実装方針
- `Authorization: Bearer <token>` ヘッダで API 呼び出し
- React Context で認証状態を保持
- TanStack Query のグローバル fetch ラッパーでヘッダを自動付与
- 401 レスポンス時はトークンを破棄してログイン画面へ遷移

### 2.5 CORS 設定
- 許可オリジン(本番):`https://<bridge-vercel-url>`
- 許可オリジン(開発):`http://localhost:5173`
- 許可メソッド:`GET, POST, PATCH, DELETE, OPTIONS`
- 許可ヘッダ:`Authorization`, `Content-Type`
- `AllowCredentials` は **false**(Cookie を使わないため)

### 2.6 将来拡張
- リフレッシュトークン方式(HttpOnly Cookie 保管、15分アクセス + 7日リフレッシュ)
- トークン失効リスト(ログアウト即時無効化)

---

## 3. 共通仕様

### 3.1 エラーレスポンス

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "リクエストボディが不正です",
    "details": [
      { "field": "email", "message": "メールアドレスの形式が不正です" }
    ]
  }
}
```

### 3.2 ページング

```
GET /resources?page=1&limit=20
```

レスポンス:
```json
{
  "items": [...],
  "pagination": {
    "page": 1,
    "limit": 20,
    "total": 47
  }
}
```

### 3.3 権限表記(本書内)
- **Anonymous**:未認証でも可
- **Authenticated**:認証済みなら誰でも
- **Sales**:営業ロール
- **Engineer**:技術者ロール
- **Admin**:管理者ロール
- **Self**:対象リソースの所有者本人

### 3.4 `me` の取り扱い
- パスに含まれる `me` は、現在認証されているユーザーの ID を指す別名
- 例:`/engineers/me` は Engineer ロールのユーザーが叩いた場合、JWT ペイロードの `engineerId` に対応する Engineer レコードを返す
- ロール不一致時は `403 Forbidden`(例:Sales ロールが `/engineers/me` を叩いた場合)

---

## 4. エンドポイント一覧

### 4.1 認証
| Method | Path | 説明 | 権限 |
|---|---|---|---|
| POST | `/auth/login` | ログイン(JWT発行) | Anonymous |
| POST | `/auth/logout` | ログアウト(クライアント側破棄のみ、サーバーno-op) | Authenticated |
| GET | `/auth/me` | 現在のユーザー情報 | Authenticated |

### 4.2 ユーザー・プロフィール
| Method | Path | 説明 | 権限 |
|---|---|---|---|
| POST | `/users` | ユーザー登録 | Admin |
| GET | `/sales` | 営業一覧 | Sales, Admin |
| GET | `/sales/{id}` | 営業詳細 | Sales, Admin |
| PATCH | `/sales/{id}` | 営業情報更新 | Self, Admin |
| GET | `/sales/me/expiring-contracts` | 自分の担当エンジニアで更新間近の契約 | Sales |

### 4.3 技術者
| Method | Path | 説明 | 権限 |
|---|---|---|---|
| GET | `/engineers` | 技術者一覧(フィルタ対応) | Sales, Admin |
| GET | `/engineers/{id}` | 技術者詳細(集約) | Sales, Admin, Self |
| PATCH | `/engineers/{id}` | 技術者情報更新 | Sales, Admin, Self |
| GET | `/engineers/me` | 自分の技術者詳細(集約) | Engineer |
| PATCH | `/engineers/me` | 自分の技術者情報更新 | Engineer |
| GET | `/engineers/{id}/assignments` | アサイン+契約履歴 | Sales, Admin, Self |
| GET | `/engineers/me/assignments` | 自分のアサイン+契約履歴 | Engineer |

**`/engineers` のクエリパラメータ**:
| パラメータ | 型 | 説明 |
|---|---|---|
| `primarySalesId` | int or `me` | 担当営業 ID でフィルタ |
| `available` | bool | `true` の場合、現在 `Assignment.status='active'` の参画がない技術者に絞る |
| `skillId` | int | 特定スキル保有者でフィルタ |
| `page` / `limit` | int | ページング |

### 4.4 スキル・キャリア志向
| Method | Path | 説明 | 権限 |
|---|---|---|---|
| GET | `/skills` | スキルマスタ一覧 | Authenticated |
| GET | `/engineers/{id}/skills` | 技術者の保有スキル | Sales, Admin, Self |
| PUT | `/engineers/{id}/skills` | スキル一括更新 | Sales, Admin, Self |
| GET | `/engineers/{id}/preferences` | キャリア志向 | Sales, Admin, Self |
| PUT | `/engineers/{id}/preferences` | キャリア志向更新 | Sales, Admin, Self |

### 4.5 案件
| Method | Path | 説明 | 権限 |
|---|---|---|---|
| GET | `/projects` | 案件一覧(フィルタ対応) | Authenticated |
| GET | `/projects/{id}` | 案件詳細 | Authenticated |
| POST | `/projects` | 案件作成 | Sales, Admin |
| PATCH | `/projects/{id}` | 案件更新 | Sales, Admin |
| DELETE | `/projects/{id}` | 案件削除(論理削除) | Sales, Admin |

**`/projects` のクエリパラメータ**:
| パラメータ | 型 | 説明 |
|---|---|---|
| `ownerSalesId` | int or `me` | 主担当営業 ID でフィルタ |
| `status` | enum | `open` / `closed` でフィルタ |
| `page` / `limit` | int | ページング |

### 4.6 アサイン・契約
| Method | Path | 説明 | 権限 |
|---|---|---|---|
| POST | `/assignments` | アサイン作成 | Sales, Admin |
| GET | `/assignments/{id}` | アサイン詳細 | Sales, Admin, Self |
| PATCH | `/assignments/{id}` | アサイン更新(status等) | Sales, Admin |
| POST | `/assignments/{id}/contracts` | 契約追加(新規・更新) | Sales, Admin |
| GET | `/assignments/{id}/contracts` | 単一アサインの契約履歴 | Sales, Admin, Self |
| GET | `/contracts/expiring` | 全社の更新間近契約(管理画面用) | Admin |

### 4.7 マッチング
| Method | Path | 説明 | 権限 |
|---|---|---|---|
| GET | `/projects/{id}/matches` | 案件にマッチする技術者 | Sales, Admin |
| GET | `/engineers/me/matches` | 自分にマッチする案件 | Engineer |

---

## 5. 主要レスポンス構造

### 5.1 `GET /engineers/{id}` / `GET /engineers/me`(集約)

```json
{
  "id": 7,
  "name": "田中 太郎",
  "bio": "Webフロントエンド中心のエンジニア",
  "avoidedWorkNote": "パッケージ導入は避けたい",
  "primarySales": {
    "id": 3,
    "name": "佐藤 営業",
    "department": "第一営業部",
    "email": "sato@example.com"
  },
  "skills": [
    { "skillId": 1, "skillName": "React",      "category": "framework", "years": 5 },
    { "skillId": 2, "skillName": "TypeScript", "category": "language",  "years": 4 }
  ],
  "preferredSkills": [
    { "skillId": 1, "skillName": "React", "category": "framework" }
  ],
  "preferredCategories": ["product_type", "framework"],
  "currentContract": {
    "assignmentId": 12,
    "projectId": 42,
    "projectTitle": "金融系Webアプリ開発",
    "clientName": "A銀行",
    "periodFrom": "2025-03-01",
    "periodTo": "2025-05-31",
    "unitPrice": 850000,
    "contractType": "renewal",
    "daysRemaining": 23,
    "renewalStatus": "not_planned"
  }
}
```

### 5.2 `GET /engineers/{id}/assignments` / `GET /engineers/me/assignments`

```json
{
  "assignments": [
    {
      "id": 12,
      "projectId": 42,
      "projectTitle": "金融系Webアプリ開発",
      "clientName": "A銀行",
      "status": "active",
      "assignedAt": "2024-12-01",
      "contracts": [
        {
          "id": 33,
          "periodFrom": "2025-03-01",
          "periodTo": "2025-05-31",
          "unitPrice": 850000,
          "contractType": "renewal",
          "isCurrent": true
        },
        {
          "id": 32,
          "periodFrom": "2024-12-01",
          "periodTo": "2025-02-28",
          "unitPrice": 850000,
          "contractType": "initial",
          "isCurrent": false
        }
      ]
    }
  ]
}
```

**ソート規約**:`assignments` は `assignedAt` 降順、各 `contracts` は `periodFrom` 降順。

### 5.3 `GET /projects/{id}`

```json
{
  "id": 42,
  "title": "金融系Webアプリ開発",
  "clientName": "A銀行",
  "description": "メガバンク向け投信管理システムの新規開発",
  "startDate": "2025-06-01",
  "endDate": "2025-12-31",
  "unitPriceMin": 700000,
  "unitPriceMax": 900000,
  "status": "open",
  "ownerSales": { "id": 3, "name": "佐藤 営業" },
  "requiredSkills": [
    { "skillId": 1, "skillName": "React",            "category": "framework",      "requirement": "required",  "requiredYears": 3 },
    { "skillId": 2, "skillName": "TypeScript",       "category": "language",       "requirement": "required",  "requiredYears": 2 },
    { "skillId": 3, "skillName": "C#",               "category": "language",       "requirement": "required",  "requiredYears": 5 },
    { "skillId": 5, "skillName": "金融ドメイン経験", "category": "domain",         "requirement": "required",  "requiredYears": 2 },
    { "skillId": 10, "skillName": "AWS",             "category": "infrastructure", "requirement": "preferred", "requiredYears": 1 }
  ]
}
```

### 5.4 `GET /engineers/{id}/preferences`

```json
{
  "preferredSkills": [
    { "skillId": 1, "skillName": "React",      "category": "framework" },
    { "skillId": 2, "skillName": "TypeScript", "category": "language" }
  ],
  "preferredCategories": ["product_type", "framework"],
  "avoidedWorkNote": "パッケージ導入は避けたい"
}
```

### 5.5 `GET /sales/me/expiring-contracts`

```json
{
  "items": [
    {
      "contractId": 33,
      "assignmentId": 12,
      "engineer": { "id": 7, "name": "田中 太郎" },
      "project": { "id": 42, "title": "金融系Webアプリ開発", "clientName": "A銀行" },
      "periodTo": "2025-05-31",
      "daysRemaining": 23,
      "renewalStatus": "not_planned"
    }
  ]
}
```

**クエリパラメータ**:
| パラメータ | 型 | 説明 | デフォルト |
|---|---|---|---|
| `days` | int | 残り何日以下を対象とするか | 30 |

**`renewalStatus` の判定ロジック**:

| 値 | 判定 |
|---|---|
| `scheduled` | 現契約の `period_to` 以降に始まる Contract レコードが存在 |
| `not_planned` | 現契約の `period_to` 以降の Contract レコードがない |
| `not_applicable` | アサインが `ended` または `cancelled`(レスポンスに含めない) |

### 5.6 `GET /contracts/expiring`(Admin 用)

レスポンス構造は `5.5` と同一。フィルタなしで全件返す。

---

## 6. マッチング API

### 6.1 スコアリング方針
- **ソフトスコアリング**(ハードフィルタではない)
- **0〜100 に正規化**
- MVP では **稼働中の技術者は対象外**(空きのみ)

### 6.2 スコアリング式
```
rawScore = (必須スキル一致数 × 3)
         + (歓迎スキル一致数 × 1)
         + (必須スキルの年数充足ボーナス × 0.5)
         - (必須スキルの年数不足ペナルティ × 1)
         + (希望カテゴリ一致ボーナス × 2)

score = (rawScore / maxPossibleScore) × 100
```

### 6.3 案件起点:`GET /projects/{id}/matches`

```json
{
  "projectId": 42,
  "projectTitle": "金融系Webアプリ開発",
  "maxPossibleScore": 100,
  "matches": [
    {
      "engineer": {
        "id": 7,
        "name": "田中 太郎",
        "primarySalesName": "佐藤 営業"
      },
      "score": 85,
      "scoreBreakdown": {
        "skillMatch": 55,
        "yearsAdequacy": 20,
        "categoryMatch": 10
      },
      "skillEvaluations": [
        { "skillName": "React",            "requirement": "required",  "requiredYears": 3, "actualYears": 5, "status": "matched" },
        { "skillName": "TypeScript",       "requirement": "required",  "requiredYears": 2, "actualYears": 4, "status": "matched" },
        { "skillName": "AWS",              "requirement": "preferred", "requiredYears": 1, "actualYears": 2, "status": "matched" },
        { "skillName": "C#",               "requirement": "required",  "requiredYears": 5, "actualYears": 3, "status": "insufficient" },
        { "skillName": "金融ドメイン経験", "requirement": "required",  "requiredYears": 2, "actualYears": 0, "status": "unmet" }
      ],
      "categoryPreferenceMatched": true
    }
  ],
  "pagination": { "page": 1, "limit": 20, "total": 47 }
}
```

### 6.4 技術者起点:`GET /engineers/me/matches`

```json
{
  "engineerId": 7,
  "engineerName": "田中 太郎",
  "matches": [
    {
      "project": {
        "id": 42,
        "title": "金融系Webアプリ開発",
        "clientName": "A銀行",
        "unitPriceMin": 700000,
        "unitPriceMax": 900000,
        "startDate": "2025-06-01",
        "ownerSalesName": "佐藤 営業"
      },
      "score": 85,
      "maxPossibleScore": 100,
      "scoreBreakdown": {
        "skillMatch": 55,
        "yearsAdequacy": 20,
        "categoryMatch": 10
      },
      "skillEvaluations": [ ... ],
      "categoryPreferenceMatched": true
    }
  ],
  "pagination": { "page": 1, "limit": 20, "total": 12 }
}
```

**クエリパラメータ**:
| パラメータ | 型 | 説明 | デフォルト |
|---|---|---|---|
| `limit` | int | 上位何件返すか | 20 |

### 6.5 `skillEvaluations` の仕様

- **`requirement`**:`required` | `preferred`
- **`status`**:`matched` | `insufficient` | `unmet`

| status | 判定 |
|---|---|
| `matched` | `actualYears >= requiredYears` |
| `insufficient` | `0 < actualYears < requiredYears` |
| `unmet` | 該当スキルレコードなし |

### 6.6 実装時の注意点(N+1対策)
- EF Core の `Include` / `ThenInclude` を使って事前ロード
- スキルマスタはインメモリキャッシュ推奨
- 将来拡張:一覧用(軽量)と詳細用(重い)の 2 段階 API への分割

---

## 7. OpenAPI 仕様

- **方針**:ASP.NET Core の Swashbuckle を利用して、コードから自動生成
- エンドポイント実装時に `[ProducesResponseType]` 等の属性を付与して型情報を含める
- 生成された `swagger.json` を `/swagger` エンドポイントで公開
- 必要に応じてリポジトリ内に `docs/openapi.json` として出力し、バージョン管理

---

## 8. 画面とAPIの対応表

| 画面 | 利用する主要API |
|---|---|
| 1. ログイン | `POST /auth/login` |
| 2. 営業ダッシュボード | `GET /sales/me/expiring-contracts`, `GET /projects?ownerSalesId=me`, `GET /engineers?primarySalesId=me` |
| 3. 案件一覧(営業) | `GET /projects?ownerSalesId=me` 等 |
| 4. 案件詳細(マッチング) | `GET /projects/{id}`, `GET /projects/{id}/matches` |
| 5. 案件作成/編集 | `POST /projects`, `PATCH /projects/{id}`, `GET /skills` |
| 6. 技術者一覧 | `GET /engineers?available=true` 等 |
| 7. 技術者詳細(営業) | `GET /engineers/{id}`, `GET /engineers/{id}/assignments` |
| 8. 技術者作成/編集 | `POST /users`, `PATCH /engineers/{id}`, `PUT /engineers/{id}/skills` |
| 9. エンジニアダッシュボード | `GET /engineers/me`, `GET /engineers/me/matches?limit=3` |
| 10. プロフィール編集 | `GET /engineers/me`, `PATCH /engineers/me`, `PUT /engineers/me/skills`, `PUT /engineers/me/preferences` |
| 11. 契約履歴 | `GET /engineers/me/assignments` |
| 12. 公開案件一覧(エンジニア) | `GET /projects?status=open`, `GET /engineers/me/matches` |
| 13. 案件詳細(エンジニア) | `GET /projects/{id}` |
| 14. マッチング一覧 | `GET /engineers/me/matches` |

---

## 9. 将来拡張
- マッチング結果のキャッシュ(Redis等)
- マッチングの一覧用/詳細用 API 分割
- リフレッシュトークン方式の認証(HttpOnly Cookie)
- トークン失効リスト(ログアウト即時無効化)
- レート制限(1分あたりのリクエスト数上限)
- 監査ログ(誰がいつ何を更新したか)
- `Project.status` に `cancelled` を含めた UI 対応
