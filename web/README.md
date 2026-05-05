# Bridge Web

Bridge のフロントエンドです。React + TypeScript + Vite で構成し、API は `VITE_API_BASE_URL` で指定した ASP.NET Core API に接続します。

## Stack

- React 19
- TypeScript 6
- Vite 8
- MUI 9 / MUI X DataGrid
- TanStack Query
- React Router

## Setup

```bash
npm ci
cp .env.example .env.local
npm run dev
```

`.env.local`:

```env
VITE_API_BASE_URL=http://localhost:5130
```

Production / Vercel では以下を設定します。

```env
VITE_API_BASE_URL=https://bridge-api-mk.fly.dev
```

`VITE_*` はビルド時に埋め込まれるため、Vercel 側で変更したら Redeploy が必要です。

## Commands

```bash
npm run dev          # 開発サーバー
npx tsc --noEmit     # 型チェック
npm run lint         # ESLint
npm run build        # 本番ビルド
npm run preview      # build 結果の確認
```

## Authentication

- `/auth/login` で JWT を取得します。
- JWT とユーザー情報は `sessionStorage` に保持します。
- 同一タブ内のリロードではログイン状態を復元します。
- タブを閉じた場合、またはログアウト時にセッションを破棄します。

開発用ユーザー:

| Role | Email | Password |
|---|---|---|
| Sales | `sato@bridge.local` | `Sales1234!` |
| Sales | `yamada.sales@bridge.local` | `Sales1234!` |
| Engineer | `tanaka@bridge.local` | `Engineer1234!` |
| Engineer | `suzuki.hanako@bridge.local` | `Engineer1234!` |
| Engineer | `aoki.mika@bridge.local` | `Engineer1234!` |

## API Integration

共通 HTTP client は `src/shared/api/http.ts` にあります。
未設定時は開発環境では `http://localhost:5130`、本番ビルドでは `https://bridge-api-mk.fly.dev` に接続します。

主な接続済み API:

- `POST /auth/login`
- `POST /auth/logout`
- `GET /projects`
- `GET /projects/{id}`
- `POST /projects`
- `PATCH /projects/{id}`
- `GET /skills`
- `GET /engineers`
- `GET /engineers/{id}`
- `GET /projects/{id}/matches`
- `GET /engineers/me/matches`
- `GET /engineers/me/assignments`

## Deployment

Vercel の設定:

| Setting | Value |
|---|---|
| Root Directory | `web` |
| Framework Preset | Vite |
| Build Command | `npm run build` |
| Output Directory | `dist` |

詳細は [`../Docs/deployment.md`](../Docs/deployment.md) を参照してください。
