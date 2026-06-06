import { Select, Space } from 'antd';
import { getLocale, setLocale, t } from '@/app/i18n';
import { GlobalOutlined } from '@ant-design/icons';

export default function LanguageSelector() {
  const currentLocale = getLocale();

  return (
    <Space>
      <GlobalOutlined aria-hidden="true" />
      <Select
        aria-label={t('language.selector.label')}
        value={currentLocale}
        onChange={setLocale}
        options={[
          { value: 'pt-BR', label: 'Português' },
          { value: 'en-US', label: 'English' },
        ]}
        bordered={false}
        style={{ width: 110 }}
      />
    </Space>
  );
}
