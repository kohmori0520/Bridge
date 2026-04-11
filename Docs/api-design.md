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

---

## 2. 認証方式

### 2.1 方針:セッションCookie

**理由**:SPA と API が同一ドメインで動く単一サービス構成のため、JWT が必要となるマイクロサービスやモバイルクライアントのユースケースがない。実装のシンプルさと即時ログアウトの容易さを重視。

### 2.2 実装方針
- ASP.NET Core の Cookie 認証(`Microsoft.AspNetCore.Authentication.Cookies`)
- セッションストアはサーバー側(DB or 分散キャッシュ)
- **Cookie属性**:`HttpOnly` + `Secure` + `SameSite=Lax`
- **CSRF対策**:`SameSite=Lax` でカバー、状態変更系にCSRFトークンを検討
- **ログアウト**:サーバー側セッション破棄 + Cookie クリア(即時無効化が可能)

### 2.3 デプロイ上の注意
- SPA と API は **同一ドメイン** で動かす前提
- 例:`bridge.example.com` 配下で `/api/*` を API、残りを SPA にルーティング
- クロスオリジン構成は CORS + SameSite=None の複雑さが発生するため避ける

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

---

## 4. エンドポイント一覧

### 4.1 認証
| Method | Path | 説明 | 権限 |
|---|---|---|---|
| POST | `/auth/login` | ログイン | Anonymous |
| POST | `/auth/logout` | ログアウト | Authenticated |
| GET | `/auth/me` | 現在のユーザー情報 | Authenticated |

### 4.2 ユーザー・プロフィール
| Method | Path | 説明 | 権限 |
|---|---|---|---|
| POST | `/users` | ユーザー登録 | Admin |
| GET | `/sales` | 営業一覧 | Sales, Admin |
| GET | `/sales/{id}` | 営業詳細 | Sales, Admin |
| PATCH | `/sales/{id}` | 営業情報更新 | Self, Admin |

### 4.3 技術者
| Method | Path | 説明 | 権限 |
|---|---|---|---|
| GET | `/engineers` | 技術者一覧(フィルタ対応) | Sales, Admin |
| GET | `/engineers/{id}` | 技術者詳細 | Sales, Admin, Self |
| PATCH | `/engineers/{id}` | 技術者情報更新 | Sales, Admin, Self |
| GET | `/engineers/me` | 自分の技術者情報 | Engineer |
| PATCH | `/engineers/me` | 自分の技術者情報更新 | Engineer |

**クエリ例**:
```
GET /engineers?available=true&primarySalesId=3&skillId=1
```

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

### 4.6 アサイン・契約
| Method | Path | 説明 | 権限 |
|---|---|---|---|
| POST | `/assignments` | アサイン作成 | Sales, Admin |
| GET | `/assignments/{id}` | アサイン詳細 | Sales, Admin, Self |
| PATCH | `/assignments/{id}` | アサイン更新(status等) | Sales, Admin |
| POST | `/assignments/{id}/contracts` | 契約追加(新規・更新) | Sales, Admin |
| GET | `/assignments/{id}/contracts` | 契約履歴一覧 | Sales, Admin, Self |
| GET | `/contracts/expiring` | 終了1ヶ月前アラート対象 | Sales, Admin |

### 4.7 マッチング
| Method | Path | 説明 | 権限 |
|---|---|---|---|
| GET | `/projects/{id}/matches` | 案件にマッチする技術者 | Sales, Admin |
| GET | `/engineers/me/matches` | 自分にマッチする案件 | Engineer |

---

## 5. マッチング API の詳細設計

### 5.1 スコアリング方針
- **ソフトスコアリング**(ハードフィルタではない)
- **0〜100 に正規化**(案件ごとに最大可能スコアで割る)
- MVP では **稼働中の技術者は対象外**(空きのみ)

### 5.2 スコアリング式(初版・実装時に調整)
```
rawScore = (必須スキル一致数 × 3)
         + (歓迎スキル一致数 × 1)
         + (必須スキルの年数充足ボーナス × 0.5)
         - (必須スキルの年数不足ペナルティ × 1)
         + (希望カテゴリ一致ボーナス × 2)

score = (rawScore / maxPossibleScore) × 100
```

### 5.3 案件起点:`GET /projects/{id}/matches`

**レスポンス**:
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
        { "skillName": "React",         "requirement": "required",  "requiredYears": 3, "actualYears": 5, "status": "matched" },
        { "skillName": "TypeScript",    "requirement": "required",  "requiredYears": 2, "actualYears": 4, "status": "matched" },
        { "skillName": "AWS",           "requirement": "preferred", "requiredYears": 1, "actualYears": 2, "status": "matched" },
        { "skillName": "C#",            "requirement": "required",  "requiredYears": 5, "actualYears": 3, "status": "insufficient" },
        { "skillName": "金融ドメイン経験", "requirement": "required",  "requiredYears": 2, "actualYears": 0, "status": "unmet" }
      ],
      "categoryPreferenceMatched": true
    }
  ],
  "pagination": { "page": 1, "limit": 20, "total": 47 }
}
```

### 5.4 技術者起点:`GET /engineers/me/matches`

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

**案件起点との違い**:`maxPossibleScore` は各 match 内に入る(案件ごとに最大スコアが異なるため)。

### 5.5 `skillEvaluations` の仕様

2軸で表現:
- **`requirement`**:`required` | `preferred`(案件側の要求種別)
- **`status`**:`matched` | `insufficient` | `unmet`(技術者側の充足度)

| status | 意味 | 判定 |
|---|---|---|
| `matched` | 要求を満たしている | `actualYears >= requiredYears` |
| `insufficient` | 持っているが年数不足 | `0 < actualYears < requiredYears` |
| `unmet` | そもそも持っていない | 該当スキルレコードなし |

### 5.6 設計判断:4配列を1配列に統合した理由
- ドメインモデルとして自然(スキル1つに対する評価が1エントリ)
- フロントで自由に filter できる
- 将来の拡張(新しい status 値の追加)に対応しやすい
- トップレベル構造がシンプル

### 5.7 実装時の注意点(N+1対策)
- 技術者ごとに保有スキル、案件ごとに要求スキルを取得する必要があり、素朴な実装では N+1 クエリが発生する
- EF Core の `Include` / `ThenInclude` を使って事前ロード
- スキルマスタは変更が稀なため、インメモリキャッシュも検討価値あり
- **大量データ時の代替設計**:一覧用(軽量)と詳細用(重い)の 2 段階 API への分割(将来拡張)

---

## 6. OpenAPI 仕様

- **方針**:ASP.NET Core の Swashbuckle を利用して、コードから自動生成
- エンドポイント実装時に `[ProducesResponseType]` 等の属性を付与して型情報を含める
- 生成された `swagger.json` を `/swagger` エンドポイントで公開
- 必要に応じてリポジトリ内に `docs/openapi.json` として出力し、バージョン管理

---

## 7. 将来拡張
- マッチング結果のキャッシュ(Redis等)
- マッチングの一覧用/詳細用 API 分割
- CSRF トークンによる明示的な保護
- レート制限(1分あたりのリクエスト数上限)
- 監査ログ(誰がいつ何を更新したか)
