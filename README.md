# Bridge

SES業界の「営業とエンジニアの情報非対称性」を解消するための業務管理システムです。営業は案件・エンジニア・アサイン・契約を管理し、エンジニアは自身のスキル、希望、契約状況、マッチング案件を確認できます。

## 構成

```text
Bridge/
├── api/   # ASP.NET Core Web API
├── web/   # React + Vite SPA
└── Docs/  # 要件・API・画面・デプロイ設計
```

## Tech Stack

### Backend

- .NET 10 / ASP.NET Core Web API
- Entity Framework Core 10
- PostgreSQL / Supabase
- JWT 認証
- Fly.io deploy

### Frontend

- React 19 / TypeScript 6 / Vite 8
- MUI 9 / MUI X DataGrid
- TanStack Query
- React Router
- Vercel deploy

## ローカル起動

### API

```bash
cd api
dotnet restore Bridge.slnx
dotnet build Bridge.slnx
dotnet test Bridge.slnx
dotnet run --project src/Bridge.Api
```

API の既定ローカル URL は `http://localhost:5130` です。

ローカル実行には `api/src/Bridge.Api/appsettings.Development.json` などで以下が必要です。このファイルは Git 管理しません。

```json
{
  "ConnectionStrings": {
    "Default": "<PostgreSQL connection string>"
  },
  "Jwt": {
    "Secret": "<32 bytes以上のローカル用 secret>",
    "Issuer": "Bridge",
    "Audience": "BridgeApi",
    "ExpiryHours": 24
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173"]
  }
}
```

### Web

```bash
cd web
npm ci
cp .env.example .env.local
npm run dev
```

`web/.env.local`:

```env
VITE_API_BASE_URL=http://localhost:5130
```

## デモログイン

Development 環境では `DemoUserSeeder` により以下のユーザーが作成されます。

| Role | Email | Password |
|---|---|---|
| Sales | `sato@bridge.local` | `Sales1234!` |
| Sales | `yamada.sales@bridge.local` | `Sales1234!` |
| Engineer | `tanaka@bridge.local` | `Engineer1234!` |
| Engineer | `suzuki.hanako@bridge.local` | `Engineer1234!` |
| Engineer | `aoki.mika@bridge.local` | `Engineer1234!` |
| Admin | `admin@bridge.local` | `Admin1234!` |

Production では Seeder は実行されません。

## 主要機能

- JWT ログイン / ログアウト / `me` 取得
- 案件一覧・詳細・作成・編集・論理削除
- 技術者一覧・詳細・プロフィール更新
- スキル・希望条件管理
- アサイン・契約管理
- 案件起点 / エンジニア起点のマッチング
- 契約更新アラート
- `/health` による死活監視

## CI/CD

- API: `.github/workflows/api-ci.yml`
  - PR: restore → build → test
  - `main` push: restore → build → test → Fly.io deploy
- Web: `.github/workflows/web-ci.yml`
  - PR / `main` push: npm ci → typecheck → lint → build
  - deploy は Vercel の GitHub 連携で実行

## デプロイ

詳細は [`Docs/deployment.md`](Docs/deployment.md) を参照してください。

概要:

- API は Fly.io (`bridge-api-mk`)
- Web は Vercel
- Secret は Git 管理せず Fly secrets / Vercel Environment Variables に設定
- Swagger は Development のみ有効
- CORS は Vercel の固定 URL を allowlist する

### 主な環境変数

| Scope | Key | Purpose |
|---|---|---|
| Web | `VITE_API_BASE_URL` | フロントエンドが接続する API のベース URL |
| API | `ConnectionStrings__Default` | PostgreSQL / Supabase 接続文字列 |
| API | `Jwt__Secret` | JWT 署名 secret |
| API | `Jwt__Issuer` | JWT issuer |
| API | `Jwt__Audience` | JWT audience |
| API | `Jwt__ExpiryHours` | JWT 有効期限 |
| API | `Cors__AllowedOriginsCsv` | 許可する Web origin のカンマ区切り |
| API | `Cors__AllowVercelPreviewOrigins` | Vercel preview URL を許可するか |

フロントエンドでは JWT とユーザー情報を `sessionStorage` に保持します。ブラウザのタブを閉じるとセッションは破棄され、ログアウト時にも明示的に削除します。

## ドキュメント

- [`Docs/requirements.md`](Docs/requirements.md): 要件定義
- [`Docs/er-diagram.md`](Docs/er-diagram.md): ER 図
- [`Docs/api-design.md`](Docs/api-design.md): API 設計
- [`Docs/screen-design.md`](Docs/screen-design.md): 画面設計
- [`Docs/deployment.md`](Docs/deployment.md): デプロイ・環境変数・トラブルシュート

## 注意

- `appsettings.Development.json`, `.env.local`, DB 接続文字列, JWT secret はコミットしないでください。
- `fly.toml`, GitHub Actions, Dockerfile は secret を含まない設定ファイルとして Git 管理します。
- Vite の `VITE_*` 環境変数はビルド時に埋め込まれるため、Vercel で変更したら再デプロイが必要です。
