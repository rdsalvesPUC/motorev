import { useEffect, useMemo, useState } from 'react';
import { Typography, Form, Input, InputNumber, Select, Button, Flex, message, Spin, AutoComplete, theme } from 'antd';
import { CarOutlined, ArrowLeftOutlined } from '@ant-design/icons';
import DashboardBreadcrumb from '@/app/components/layout/DashboardBreadcrumb';
import { linhaService } from '@/app/services/linhaService';
import { modeloMotoService } from '@/app/services/modeloMotoService';
import { Linha } from '@/app/models/Linha';
import { ModeloMotoRequest } from '@/app/models/ModeloMotoRequest';
import { handleApiError } from '@/app/utils/errorHandler';
import { PATH_SEGMENTS } from '@/app/paths';
import { getLocale, t } from '@/app/i18n';

const { Title } = Typography;

interface CatalogoMotosCreateProps {
  onBack: () => void;
}

const currentYear = new Date().getFullYear();
const minModelYear = 1901;

export default function FormCatalogoModeloMoto({ onBack }: CatalogoMotosCreateProps) {
  const [form] = Form.useForm<ModeloMotoRequest>();
  const { token } = theme.useToken();
  const [linhas, setLinhas] = useState<Linha[]>([]);
  const [marcas, setMarcas] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const fetchOptions = async () => {
      try {
        setLoading(true);
        const [linhasData, modelosData] = await Promise.all([
          linhaService.getAll(true),
          modeloMotoService.getCatalogo(),
        ]);

        setLinhas(linhasData);
        setMarcas(modelosData.map((modelo) => modelo.marca));
      } catch (error) {
        handleApiError(error, 'error.modeloMotoOptionsLoad');
      } finally {
        setLoading(false);
      }
    };

    fetchOptions();
  }, []);

  const marcaOptions = useMemo(() => {
    return Array.from(new Set(marcas.map((marca) => marca.trim()).filter(Boolean)))
      .sort((a, b) => a.localeCompare(b, getLocale()))
      .map((marca) => ({ value: marca, label: marca }));
  }, [marcas]);

  const handleSubmit = async (values: ModeloMotoRequest) => {
    try {
      setLoading(true);
      await modeloMotoService.create(values);
      message.success(t('modeloMotoCreatedSuccess'));
      form.resetFields();
      onBack();
    } catch (error) {
      handleApiError(error, 'error.createModeloMoto', { conflictKey: 'error.modeloMotoConflict' });
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = () => {
    form.resetFields();
    onBack();
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
                icon: <CarOutlined />,
              },
              {
                title: t('modeloMotoCatalog.title'),
                path: `/dashboard/concessionaria/${PATH_SEGMENTS.CONCESSIONARIA_CATALOGOS_MOTOS}`,
              },
              {
                title: t('modeloMotoCatalog.new'),
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
              {t('modeloMotoCatalog.registerNew')}
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
              label={t('modeloMotoCatalog.brand')}
              name="marca"
              rules={[{ required: true, message: t('modeloMotoCatalog.brandRequired') }]}
            >
              <AutoComplete
                placeholder={t('modeloMotoCatalog.brandPlaceholder')}
                options={marcaOptions}
                filterOption={(inputValue, option) =>
                  String(option?.value ?? '').toLowerCase().includes(inputValue.toLowerCase())
                }
              />
            </Form.Item>

            <Form.Item
              label={t('modeloMotoCatalog.model')}
              name="nomeModelo"
              rules={[
                { required: true, message: t('modeloMotoCatalog.modelRequired') },
                { max: 100, message: t('modeloMotoCatalog.modelLength') },
              ]}
            >
              <Input placeholder={t('modeloMotoCatalog.modelPlaceholder')} />
            </Form.Item>

            <Form.Item
              label={t('modeloMotoCatalog.year')}
              name="ano"
              rules={[
                {
                  type: 'number',
                  min: minModelYear,
                  max: currentYear,
                  message: t('modeloMotoCatalog.yearRange', { min: minModelYear, max: currentYear }),
                },
              ]}
            >
              <InputNumber
                placeholder={t('modeloMotoCatalog.yearPlaceholder')}
                min={minModelYear}
                max={currentYear}
                precision={0}
                style={{ width: '100%' }}
                parser={(value) => value?.replace(/[^\d]/g, '') as any}
              />
            </Form.Item>

            <Form.Item
              label={t('modeloMotoCatalog.engine')}
              name="cilindrada"
              rules={[{ max: 50, message: t('modeloMotoCatalog.engineLength') }]}
            >
              <Input placeholder={t('modeloMotoCatalog.enginePlaceholder')} />
            </Form.Item>

            <Form.Item
              label={t('modeloMotoCatalog.line')}
              name="linhaId"
              rules={[{ required: true, message: t('modeloMotoCatalog.lineRequired') }]}
            >
              <Select
                placeholder={t('modeloMotoCatalog.linePlaceholder')}
                options={linhas.map((linha) => ({ value: linha.id, label: linha.nome }))}
              />
            </Form.Item>

            <Form.Item style={{ marginBottom: 0 }}>
              <Flex gap="middle">
                <Button type="primary" htmlType="submit" size="large" loading={loading}>
                  {t('modeloMotoCatalog.save')}
                </Button>
                <Button size="large" onClick={handleCancel}>
                  {t('modeloMotoCatalog.cancel')}
                </Button>
              </Flex>
            </Form.Item>
          </Form>
        </div>
      </Flex>
    </Spin>
  );
}
