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
