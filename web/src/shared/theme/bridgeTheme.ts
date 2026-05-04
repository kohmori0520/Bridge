import { createTheme } from '@mui/material'

export const bridgeTheme = createTheme({
  palette: {
    primary: { main: '#12345b', dark: '#0b223d', light: '#315d8f' },
    warning: { main: '#f08c2e' },
    background: { default: '#f4f7fb' },
  },
  typography: {
    fontFamily: '"Noto Sans JP", Roboto, "Helvetica Neue", Arial, sans-serif',
    h4: { fontWeight: 700, fontSize: '1.7rem' },
    h5: { fontWeight: 700, fontSize: '1.25rem' },
    h6: { fontWeight: 700 },
    button: { fontWeight: 700, textTransform: 'none' },
  },
  shape: { borderRadius: 8 },
  components: {
    MuiCard: {
      styleOverrides: {
        root: { boxShadow: '0 1px 3px rgba(18, 52, 91, 0.08)' },
      },
    },
    MuiButton: {
      defaultProps: { size: 'small' },
    },
  },
})
