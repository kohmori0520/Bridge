import { CloudUpload, ContentCopy, Download } from '@mui/icons-material'
import {
  Alert,
  AlertTitle,
  Button,
  Chip,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useRef, useState } from 'react'
import { PageHeader } from '../../../shared/components/PageHeader'
import { Section } from '../../../shared/components/Section'
import { useNotify } from '../../../shared/notifications/useNotify'
import { importApi, type ImportResult } from '../api/importApi'

export function ImportPage() {
  const notify = useNotify()
  const queryClient = useQueryClient()
  const fileInputRef = useRef<HTMLInputElement>(null)
  const [file, setFile] = useState<File | null>(null)
  const [preview, setPreview] = useState<ImportResult | null>(null)
  const [committed, setCommitted] = useState<ImportResult | null>(null)
  const [error, setError] = useState<string | null>(null)

  const templateMutation = useMutation({
    mutationFn: importApi.downloadTemplate,
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = 'bridge-import-template.xlsx'
      anchor.click()
      URL.revokeObjectURL(url)
    },
    onError: (err) => setError(err.message),
  })

  const previewMutation = useMutation({
    mutationFn: (target: File) => importApi.uploadExcel(target, true),
    onSuccess: setPreview,
    onError: (err) => setError(err.message),
  })

  const commitMutation = useMutation({
    mutationFn: (target: File) => importApi.uploadExcel(target, false),
    onSuccess: async (result) => {
      setCommitted(result)
      setPreview(null)
      setFile(null)
      notify('取り込みが完了しました')
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['engineers'] }),
        queryClient.invalidateQueries({ queryKey: ['projects'] }),
      ])
    },
    onError: (err) => setError(err.message),
  })

  const handleFileChange = (selected: File | null) => {
    setError(null)
    setPreview(null)
    setCommitted(null)
    setFile(selected)
    if (selected) previewMutation.mutate(selected)
  }

  const handleCopyAccounts = async (result: ImportResult) => {
    const csv = ['メールアドレス,氏名,初期パスワード']
      .concat(result.createdAccounts.map((a) => `${a.email},${a.name},${a.initialPassword}`))
      .join('\n')
    await navigator.clipboard.writeText(csv)
    notify('アカウント一覧をコピーしました')
  }

  const busy = previewMutation.isPending || commitMutation.isPending

  return (
    <Stack spacing={3}>
      <PageHeader
        title="Excelインポート"
        subtitle="技術者・案件・契約をテンプレートから一括登録します"
        actions={
          <Button
            variant="outlined"
            startIcon={<Download />}
            onClick={() => templateMutation.mutate()}
            disabled={templateMutation.isPending}
          >
            テンプレートをダウンロード
          </Button>
        }
      />

      <Section title="1. ファイルを選択">
        <Stack spacing={2} sx={{ alignItems: 'flex-start' }}>
          <Typography color="text.secondary">
            テンプレートに記入した .xlsx ファイルを選択すると、自動で内容を検証します。エラーがなければ取り込みを確定できます。
          </Typography>
          <Button component="label" variant="contained" startIcon={<CloudUpload />} disabled={busy}>
            ファイルを選択
            <input
              ref={fileInputRef}
              type="file"
              hidden
              accept=".xlsx"
              onChange={(event) => handleFileChange(event.target.files?.[0] ?? null)}
            />
          </Button>
          {file && <Chip label={file.name} onDelete={() => handleFileChange(null)} />}
        </Stack>
      </Section>

      {error && (
        <Alert severity="error" onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {preview && (
        <Section title="2. 検証結果">
          <Stack spacing={2}>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>シート</TableCell>
                  <TableCell align="right">新規</TableCell>
                  <TableCell align="right">更新</TableCell>
                  <TableCell align="right">エラー</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {preview.sheets.map((sheet) => (
                  <TableRow key={sheet.sheet}>
                    <TableCell>{sheet.sheet}</TableCell>
                    <TableCell align="right">{sheet.newCount}</TableCell>
                    <TableCell align="right">{sheet.updateCount}</TableCell>
                    <TableCell align="right">{sheet.errorCount}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>

            {preview.valid ? (
              <>
                <Alert severity="success">エラーはありません。取り込みを確定できます。</Alert>
                <Button
                  variant="contained"
                  color="primary"
                  disabled={!file || busy}
                  onClick={() => file && commitMutation.mutate(file)}
                >
                  取り込みを確定
                </Button>
              </>
            ) : (
              <>
                <Alert severity="warning">
                  エラーがあるため取り込めません。Excel を修正して再度ファイルを選択してください(エラーが1件でもあると何も取り込まれません)。
                </Alert>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>シート</TableCell>
                      <TableCell>行</TableCell>
                      <TableCell>内容</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {preview.errors.map((rowError, index) => (
                      <TableRow key={index}>
                        <TableCell>{rowError.sheet}</TableCell>
                        <TableCell>{rowError.row > 0 ? rowError.row : '-'}</TableCell>
                        <TableCell>{rowError.message}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </>
            )}
          </Stack>
        </Section>
      )}

      {committed && (
        <Section title="3. 取り込み完了">
          <Stack spacing={2}>
            {committed.createdAccounts.length > 0 ? (
              <>
                <Alert severity="warning">
                  <AlertTitle>新規アカウントの初期パスワード</AlertTitle>
                  この一覧は<strong>この画面でしか表示されません</strong>。コピーして各エンジニア本人に安全な方法で伝えてください。
                </Alert>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>メールアドレス</TableCell>
                      <TableCell>氏名</TableCell>
                      <TableCell>初期パスワード</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {committed.createdAccounts.map((account) => (
                      <TableRow key={account.email}>
                        <TableCell>{account.email}</TableCell>
                        <TableCell>{account.name}</TableCell>
                        <TableCell sx={{ fontFamily: 'monospace' }}>{account.initialPassword}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
                <Button
                  variant="outlined"
                  startIcon={<ContentCopy />}
                  onClick={() => handleCopyAccounts(committed)}
                  sx={{ alignSelf: 'flex-start' }}
                >
                  CSV形式でコピー
                </Button>
              </>
            ) : (
              <Alert severity="success">取り込みが完了しました。新規アカウントの作成はありません。</Alert>
            )}
          </Stack>
        </Section>
      )}
    </Stack>
  )
}
