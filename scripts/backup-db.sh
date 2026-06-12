#!/usr/bin/env bash
# Supabase PostgreSQL の手動バックアップ。
# 使い方:
#   export DATABASE_URL="postgresql://user:pass@host:5432/postgres"
#   ./scripts/backup-db.sh [出力ディレクトリ]
#
# リストア:
#   pg_restore --clean --if-exists --no-owner -d "$DATABASE_URL" <dumpファイル>
set -euo pipefail

: "${DATABASE_URL:?DATABASE_URL に Supabase の接続文字列を設定してください}"

out_dir="${1:-backups}"
mkdir -p "$out_dir"

timestamp="$(date +%Y%m%d_%H%M%S)"
out_file="${out_dir}/bridge_${timestamp}.dump"

pg_dump "$DATABASE_URL" --format=custom --no-owner --file "$out_file"

echo "Backup written to ${out_file}"
