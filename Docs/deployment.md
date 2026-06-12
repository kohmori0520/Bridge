# Deployment Guide

Bridge は API を Fly.io、Web を Vercel に分けてデプロイします。API と Web は別オリジンになるため、Vercel の URL を API 側 CORS allowlist に登録する必要があります。

## 構成

| Component | Platform | Directory |
|---|---|---|
| API | Fly.io | `api/` |
| Web | Vercel | `web/` |
| DB | Supabase PostgreSQL | external |

## API: Fly.io

### App

`api/fly.toml`:

```toml
app = "bridge-api-mk"
primary_region = "nrt"
```

`fly.toml` は app 名、region、port、health check などの公開してよい設定だけを持ちます。DB 接続文字列や JWT secret は書きません。

### Required Secrets

Fly secrets に以下を設定します。

```bash
fly secrets set -a bridge-api-mk \
  ConnectionStrings__Default="<PostgreSQL connection string>" \
  Jwt__Secret="<32 bytes以上のランダム値>" \
  Cors__AllowedOrigins__0="https://<your-vercel-domain>"
```

必要に応じてローカル開発 origin も追加します。

```bash
fly secrets set -a bridge-api-mk \
  Cors__AllowedOrigins__0="https://<your-vercel-domain>" \
  Cors__AllowedOrigins__1="http://localhost:5173"
```

Vercel preview URL は deploy ごとに変わるため、Production URL または custom domain を CORS に登録する運用を推奨します。

### Deploy

GitHub Actions から `main` push 時にデプロイします。

```yaml
flyctl deploy --remote-only --config fly.toml
```

手動デプロイする場合:

```bash
cd api
fly deploy --remote-only --config fly.toml
```

### Database Migrations

Production では起動時に未適用のマイグレーションを自動適用します(`fly.toml` の `Database__MigrateOnStartup = "true"`)。デプロイのたびにスキーマが追従するため、手動の `dotnet ef database update` は不要です。

- 適用されたマイグレーションは起動ログに `Applying N pending migration(s): ...` と出力されます
- 単一マシン運用が前提です。複数マシンに増やす場合は同時適用を避ける仕組み(デプロイ前のCIステップ化など)に切り替えてください
- 破壊的な変更(カラム削除など)を含むマイグレーションは、デプロイ前にバックアップを取得してください(後述)

ローカル開発では従来どおり手動で適用します:

```bash
cd api && dotnet ef database update --project src/Bridge.Infrastructure --startup-project src/Bridge.Api
```

### Health Check

```bash
curl https://bridge-api-mk.fly.dev/health
```

期待値:

```json
{
  "status": "ok",
  "timestamp": "...",
  "version": "1.0.0"
}
```

### Swagger

Swagger は `Development` 環境のみ有効です。Production では `/swagger` が 404 になりますが、これは正常です。

Production で `ASPNETCORE_ENVIRONMENT=Development` に変更しないでください。開発用 Seeder なども動く可能性があり危険です。

## Web: Vercel

### Project Settings

Vercel で GitHub repository を import し、以下を設定します。

| Setting | Value |
|---|---|
| Root Directory | `web` |
| Framework Preset | Vite |
| Build Command | `npm run build` |
| Output Directory | `dist` |

### Environment Variables

Vercel の Project Settings に設定します。

```text
VITE_API_BASE_URL=https://bridge-api-mk.fly.dev
```

`VITE_*` はビルド時に埋め込まれるため、変更後は Redeploy が必要です。

### SPA Routing (vercel.json)

`web/vercel.json` に rewrite を設定しています。存在しないパスへのリクエストはすべて `index.html` にフォールバックし、React Router がクライアント側でルーティングします。

```json
{
  "rewrites": [{ "source": "/(.*)", "destination": "/index.html" }]
}
```

この設定がないと、`/projects/1` などをリロードした際に Vercel が 404 を返します。

### Deploy

Web は Vercel の GitHub 連携で自動デプロイします。GitHub Actions では typecheck / lint / build の検証のみ行います。

## CORS Verification

Vercel domain を origin として preflight を確認します。

```bash
curl -i -X OPTIONS "https://bridge-api-mk.fly.dev/auth/login" \
  -H "Origin: https://<your-vercel-domain>" \
  -H "Access-Control-Request-Method: POST" \
  -H "Access-Control-Request-Headers: content-type"
```

期待されるレスポンス例:

```text
HTTP/2 204
access-control-allow-origin: https://<your-vercel-domain>
access-control-allow-methods: POST
access-control-allow-headers: content-type
```

ログイン API の確認:

```bash
curl -i -X POST "https://bridge-api-mk.fly.dev/auth/login" \
  -H "Origin: https://<your-vercel-domain>" \
  -H "Content-Type: application/json" \
  -d '{"email":"tanaka@bridge.local","password":"Engineer1234!"}'
```

## Common Issues

### 深い URL をリロードすると 404 になる

例:

```text
GET https://<your-vercel-domain>/projects/1 404
```

原因:

- `web/vercel.json` の SPA rewrite が未設定、またはデプロイに含まれていない

対応:

- `web/vercel.json` がリポジトリに含まれていることを確認
- Redeploy 後、`curl -I https://<your-vercel-domain>/projects/1` が `200` になることを確認

### Vercel から `http://localhost:5130` を叩いている

原因:

- `VITE_API_BASE_URL` が Vercel に設定されていない
- `API_BASE_URL` のように `VITE_` prefix がない
- 環境変数設定後に Redeploy していない

対応:

```text
VITE_API_BASE_URL=https://bridge-api-mk.fly.dev
```

を Vercel に設定し、Redeploy します。

### Vercel 自身の `/auth/login` に POST して 404 になる

例:

```text
POST https://bridge-gamma-roan.vercel.app/auth/login 404
```

原因:

- API base URL が空または相対 URL としてビルドされている

対応:

- Vercel の `VITE_API_BASE_URL` を確認
- Production / Preview のどちらに設定されているか確認
- Redeploy

### `No Access-Control-Allow-Origin` が出る

原因:

- Fly の `Cors__AllowedOrigins__0` とブラウザ origin が一致していない
- Vercel preview URL が変わった

対応:

```bash
fly secrets set -a bridge-api-mk \
  Cors__AllowedOrigins__0="https://<current-vercel-origin>"
```

固定の Production domain または custom domain を使うと運用が安定します。

### `/swagger` が 404

Production では正常です。Swagger は Development のみ有効です。

### `fly secrets set` で app name エラー

例:

```text
the config for your app is missing an app name
```

原因:

- `api/` 以外のディレクトリで実行している

対応:

```bash
fly secrets set -a bridge-api-mk KEY=value
```

または:

```bash
cd api
fly secrets set KEY=value
```

## Database Backup & Restore

DB は Supabase PostgreSQL です。実データ(エンジニアの個人情報・単価・契約)を載せる前に、バックアップ体制を確認してください。

### Supabase 側の自動バックアップ

- **Free プランには自動バックアップがありません**。実運用では Pro プラン(日次バックアップ・7日保持)以上を推奨します
- プランと保持期間は Supabase Dashboard → Database → Backups で確認できます

### 手動バックアップ(pg_dump)

プランに関わらず、破壊的マイグレーションの前や定期実行用に `scripts/backup-db.sh` を使えます:

```bash
export DATABASE_URL="postgresql://<user>:<password>@<host>:5432/postgres"
./scripts/backup-db.sh            # backups/bridge_YYYYMMDD_HHMMSS.dump に出力
```

- 出力先 `backups/` は `.gitignore` 済みです。**ダンプには個人情報が含まれるため、Git や共有ストレージに置かないでください**
- `pg_dump` のバージョンは Supabase の PostgreSQL メジャーバージョンに合わせてください

### リストア

```bash
pg_restore --clean --if-exists --no-owner -d "$DATABASE_URL" backups/bridge_YYYYMMDD_HHMMSS.dump
```

**運用開始前に一度リストア演習を行い、復旧できることを確認してください。**(空の Supabase プロジェクトを一時的に作って戻すのが安全です)

## Security Notes

- `appsettings.Development.json`, `.env.local`, DB 接続文字列, JWT secret は Git に載せない。
- `Jwt__Secret` は 32 bytes 以上、できれば十分長いランダム値を使う。
- Swagger は Production で公開しない。
- CORS は `AllowAnyOrigin` ではなく、Vercel の固定 URL を allowlist する。
- `fly.toml`, `Dockerfile`, GitHub Actions は secret を含めない前提で Git 管理する。

## CI/CD

API:

```text
restore -> build -> test -> deploy to Fly.io
```

Web:

```text
npm ci -> typecheck -> lint -> build
```

Web の deploy は Vercel 側で行います。
