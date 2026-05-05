import { RestartAlt, Search } from '@mui/icons-material'
import { Button, FormControl, InputAdornment, InputLabel, MenuItem, Paper, Select, TextField, Typography } from '@mui/material'

type ToolbarPanelProps = {
  keyword: string
  status: string
  statusOptions: { value: string; label: string }[]
  onKeywordChange: (value: string) => void
  onStatusChange: (value: string) => void
  keywordPlaceholder?: string
  statusLabel?: string
  resultLabel?: string
}

export function ToolbarPanel({
  keyword,
  status,
  statusOptions,
  onKeywordChange,
  onStatusChange,
  keywordPlaceholder = 'キーワード',
  statusLabel = 'ステータス',
  resultLabel,
}: ToolbarPanelProps) {
  const hasFilter = keyword.trim().length > 0 || status !== 'all'

  return (
    <Paper className="toolbar-panel" variant="outlined">
      <TextField
        size="small"
        label="キーワード"
        placeholder={keywordPlaceholder}
        value={keyword}
        onChange={(event) => onKeywordChange(event.target.value)}
        slotProps={{
          input: {
            startAdornment: (
              <InputAdornment position="start">
                <Search fontSize="small" />
              </InputAdornment>
            ),
          },
        }}
      />
      <FormControl size="small">
        <InputLabel id="status-filter-label">{statusLabel}</InputLabel>
        <Select labelId="status-filter-label" label={statusLabel} value={status} onChange={(event) => onStatusChange(event.target.value)}>
          {statusOptions.map((option) => (
            <MenuItem key={option.value} value={option.value}>
              {option.label}
            </MenuItem>
          ))}
        </Select>
      </FormControl>
      <Button
        startIcon={<RestartAlt />}
        variant="outlined"
        disabled={!hasFilter}
        onClick={() => {
          onKeywordChange('')
          onStatusChange('all')
        }}
      >
        クリア
      </Button>
      {resultLabel && (
        <Typography className="toolbar-result" variant="body2" color="text.secondary">
          {resultLabel}
        </Typography>
      )}
    </Paper>
  )
}
