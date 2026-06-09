import { useState } from 'react';
import { ArrowLeftOutlined, ToolOutlined } from '@ant-design/icons';
import { Button, Flex, Form, Input, message, Spin, theme, Typography } from 'antd';
import DashboardBreadcrumb from '@/app/components/layout/DashboardBreadcrumb';
import { PATH_SEGMENTS } from '@/app/paths';
import { linhaService } from '@/app/services/linhaService';
import { handleApiError } from '@/app/utils/errorHandler';
import { t } from '@/app/i18n';

const { Title } = Typography;

interface CatalogoLinhasCreateProps {
  onCancel?: () => void;
}

interface LinhaFormValues {
  nome: string;
  descricao?: string;
}

export default function CatalogoLinhasCreate({ onCancel }: CatalogoLinhasCreateProps) {
  const [form] = Form.useForm<LinhaFormValues>();
  const { token } = theme.useToken();
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (values: LinhaFormValues) => {
    try {
      setLoading(true);
      await linhaService.create({
        nome: values.nome,
        descricao: values.descricao,
      });
      message.success(t('linhaCreatedSuccess'));
      onCancel?.();
    } catch (error) {
      handleApiError(error, 'error.createLinha');
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = () => {
    form.resetFields();
    onCancel?.();
  };

  return (
    <Spin spinning={loading}>
      <Flex vertical gap="large" style={{ width: '100%' }}>
        <Flex vertical gap="middle" style={{ width: '100%' }}>
          <DashboardBreadcrumb
            userType="concessionaria"
            items={[
              {
                title: t('dashboard.menu.catalogos'),
                icon: <ToolOutlined />,
              },
              {
                title: t('lineCatalog.title'),
                path: PATH_SEGMENTS.CONCESSIONARIA_CATALOGOS_LINHAS,
              },
              {
                title: t('lineCatalog.newLine'),
              },
            ]}
          />

          <Flex align="center" gap="middle">
            <Button
              type="default"
              icon={<ArrowLeftOutlined />}
              onClick={handleCancel}
              aria-label={t('back')}
            />
            <Title level={2} style={{ margin: 0 }}>
              {t('lineCatalog.registerNewLine')}
            </Title>
          </Flex>
        </Flex>

        <div
          style={{
            background: token.colorBgContainer,
            border: `1px solid ${token.colorBorderSecondary}`,
            borderRadius: token.borderRadiusLG,
            padding: token.paddingLG,
          }}
        >
          <Form
            form={form}
            layout="vertical"
            onFinish={handleSubmit}
            style={{ maxWidth: '800px' }}
          >
            <Form.Item
              label={t('lineCatalog.name')}
              name="nome"
              rules={[{ required: true, message: t('lineCatalog.nameRequired') }]}
            >
              <Input placeholder={t('lineCatalog.namePlaceholder')} />
            </Form.Item>

            <Form.Item
              label={t('lineCatalog.description')}
              name="descricao"
              rules={[{ required: true, message: t('lineCatalog.descriptionRequired') }]}
            >
              <Input.TextArea
                placeholder={t('lineCatalog.descriptionPlaceholder')}
                rows={3}
              />
            </Form.Item>

            <Form.Item style={{ marginBottom: 0 }}>
              <Flex gap="middle">
                <Button type="primary" htmlType="submit" loading={loading}>
                  {t('lineCatalog.save')}
                </Button>
                <Button onClick={handleCancel}>
                  {t('lineCatalog.cancel')}
                </Button>
              </Flex>
            </Form.Item>
          </Form>
        </div>
      </Flex>
    </Spin>
  );
}
