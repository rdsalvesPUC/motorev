import { getLocale } from '@/app/i18n';

type CurrencyFractionOptions = Pick<
  Intl.NumberFormatOptions,
  'minimumFractionDigits' | 'maximumFractionDigits'
>;

export const getDecimalSeparator = () => (getLocale() === 'pt-BR' ? ',' : '.');

export const getCurrencySymbol = () => (getLocale() === 'pt-BR' ? 'R$' : '$');

export const formatCurrency = (value: number, options: CurrencyFractionOptions = {}) => {
  const numericValue = Number(value || 0);
  const formattedValue = Math.abs(numericValue).toLocaleString(getLocale(), {
    minimumFractionDigits: options.minimumFractionDigits ?? 2,
    maximumFractionDigits: options.maximumFractionDigits ?? 2,
  });
  const spacing = getLocale() === 'pt-BR' ? ' ' : '';
  const sign = numericValue < 0 ? '-' : '';

  return `${sign}${getCurrencySymbol()}${spacing}${formattedValue}`;
};

export const formatCurrencyInput = (value?: string | number) => {
  if (value === undefined || value === null || value === '') return '';

  const decimalSeparator = getDecimalSeparator();
  const thousandSeparator = decimalSeparator === ',' ? '.' : ',';
  const [integerPart, decimalPart] = String(value).split('.');
  const formattedInteger = integerPart.replace(/\B(?=(\d{3})+(?!\d))/g, thousandSeparator);

  return decimalPart !== undefined
    ? `${formattedInteger}${decimalSeparator}${decimalPart}`
    : formattedInteger;
};

export const parseCurrencyInput = (value?: string) => {
  if (!value) return '';

  return getDecimalSeparator() === ','
    ? value.replace(/[^\d,]/g, '').replace(',', '.')
    : value.replace(/[^\d.]/g, '');
};

export const formatCPF = (value: string) => {
  const digits = value.replace(/\D/g, '');
  return digits
    .replace(/(\d{3})(\d)/, '$1.$2')
    .replace(/(\d{3})(\d)/, '$1.$2')
    .replace(/(\d{3})(\d{1,2})/, '$1-$2')
    .replace(/(-\d{2})\d+?$/, '$1');
};

export const formatCNPJ = (value: string) => {
  const digits = value.replace(/\D/g, '');
  return digits
    .replace(/(\d{2})(\d)/, '$1.$2')
    .replace(/(\d{3})(\d)/, '$1.$2')
    .replace(/(\d{3})(\d)/, '$1/$2')
    .replace(/(\d{4})(\d{1,2})/, '$1-$2')
    .replace(/(-\d{2})\d+?$/, '$1');
};

export const formatPhone = (value: string) => {
  const digits = value.replace(/\D/g, '');
  if (digits.length <= 10) {
    return digits
      .replace(/(\d{2})(\d)/, '($1) $2')
      .replace(/(\d{4})(\d{1,4})/, '$1-$2')
      .replace(/(-\d{4})\d+?$/, '$1');
  }
  return digits
    .replace(/(\d{2})(\d)/, '($1) $2')
    .replace(/(\d{5})(\d{1,4})/, '$1-$2')
    .replace(/(-\d{4})\d+?$/, '$1');
};

export const formatCEP = (value: string) => {
  const digits = value.replace(/\D/g, '');
  return digits
    .replace(/(\d{5})(\d)/, '$1-$2')
    .replace(/(-\d{3})\d+?$/, '$1');
};
