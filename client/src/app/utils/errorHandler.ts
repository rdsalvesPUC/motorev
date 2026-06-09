import { message } from 'antd';
import { ApiError } from '@/app/services/http';
import { t } from '@/app/i18n';

interface HandleApiErrorOptions {
  conflictKey?: string;
}

export const handleApiError = (
  error: unknown,
  fallbackKey: string = 'error.unexpected',
  options: HandleApiErrorOptions = {},
) => {
  if (error instanceof ApiError) {
    if (error.status === 409) {
      message.error(t(options.conflictKey ?? 'error.serviceConflict'));
    } else if (error.status === 405) {
      message.error(t('error.methodNotAllowed'));
    } else if (error.status === 422) {
      message.error(error.message);
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
