import { message } from 'antd';
import { ApiError } from '../services/http';
import { t } from '../i18n';

export const handleApiError = (error: unknown, fallbackKey: string = 'error.unexpected') => {
  if (error instanceof ApiError) {
    if (error.status === 409) {
      message.error(t('error.serviceConflict'));
    } else if (error.status === 405) {
      message.error(t('error.methodNotAllowed'));
    } else {
      console.error('API Error:', error.message, error.data);
      message.error(t('error.apiError'));
    }
    return;
  }

  // Verifica se é um erro de validação do Ant Design
  if (typeof error === 'object' && error !== null && 'errorFields' in error) {
    return;
  }

  console.error('Unexpected error:', error);
  message.error(t(fallbackKey));
};
