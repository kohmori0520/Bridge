import { createTheme } from '@mui/material'

export const bridgeTheme = createTheme({
  palette: {
    primary: { main: '#12345b', dark: '#0b223d', light: '#315d8f' },
    warning: { main: '#f08c2e' },
    background: { default: '#f4f7fb', paper: '#ffffff' },
    text: { primary: '#172033', secondary: '#5f6f86' },
  },
  typography: {
    fontFamily: '"Noto Sans JP", Roboto, "Helvetica Neue", Arial, sans-serif',
    h4: { fontWeight: 700, fontSize: '1.62rem', lineHeight: 1.25 },
    h5: { fontWeight: 700, fontSize: '1.16rem', lineHeight: 1.32 },
    h6: { fontWeight: 700 },
    body2: { lineHeight: 1.55 },
    button: { fontWeight: 700, textTransform: 'none' },
  },
  shape: { borderRadius: 8 },
  components: {
    MuiCard: {
      styleOverrides: {
        root: { boxShadow: '0 8px 24px rgba(18, 52, 91, 0.07)' },
      },
    },
    MuiButton: {
      defaultProps: { size: 'small', disableElevation: true },
    },
    MuiTooltip: {
      defaultProps: { arrow: true },
    },
    MuiChip: {
      styleOverrides: {
        root: { fontWeight: 700 },
      },
    },
    MuiTextField: {
      defaultProps: { size: 'small' },
    },
    MuiFormControl: {
      defaultProps: { size: 'small' },
    },
  },
})
