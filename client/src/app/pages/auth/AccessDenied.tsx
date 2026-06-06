import { Button, Result } from 'antd';
import { useNavigate } from 'react-router';
import { PATHS } from '@/app/paths';
import { t } from '@/app/i18n';

export default function AccessDenied() {
  const navigate = useNavigate();

  return (
    <Result
      status="403"
      title={t('accessDenied.title')}
      subTitle={t('accessDenied.subTitle')}
      extra={<Button type="primary" onClick={() => navigate(PATHS.HOME)}>{t('accessDenied.backHome')}</Button>}
    />
  );
}
