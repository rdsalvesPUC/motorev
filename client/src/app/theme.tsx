import { ConfigProvider, theme } from 'antd';
import { useConfiguracoes } from '@/app/contexts/ConfiguracoesContext';

export function AntdThemeProvider({ children }: { children: React.ReactNode }) {
  const { configuracoes } = useConfiguracoes();
  const isDark = configuracoes.tema === 'dark';

  return (
    <ConfigProvider
      theme={{
        algorithm: isDark ? theme.darkAlgorithm : theme.defaultAlgorithm,
        token: {
          fontFamily: `'SF Pro Display', 'SF Pro Text', -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', 'Oxygen', 'Ubuntu', 'Cantarell', 'Fira Sans', 'Droid Sans', 'Helvetica Neue', sans-serif`,
        },
      }}
    >
      {children}
    </ConfigProvider>
  );
}
