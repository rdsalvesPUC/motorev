import { useEffect, useState } from 'react';
import { Typography, Form, Input, InputNumber, Select, Button, Space, message, Card, Spin } from 'antd';
import { CarOutlined, ArrowLeftOutlined } from '@ant-design/icons';
import DashboardBreadcrumb from '@/app/components/layout/DashboardBreadcrumb';
import { linhaService } from '@/app/services/linhaService';
import { modeloMotoService } from '@/app/services/modeloMotoService';
import { Linha } from '@/app/models/Linha';
import { ModeloMotoRequest } from '@/app/models/ModeloMotoRequest';
import { handleApiError } from '@/app/utils/errorHandler';
import { PATH_SEGMENTS } from '@/app/paths';
import { t } from '@/app/i18n';

const { Title } = Typography;

interface CatalogoMotosCreateProps {
  onBack: () => void;
}

const marcaOptions = [
  { value: 'Honda', label: 'Honda' },
  { value: 'Yamaha', label: 'Yamaha' },
  { value: 'Suzuki', label: 'Suzuki' },
  { value: 'Kawasaki', label: 'Kawasaki' },
  { value: 'BMW', label: 'BMW' },
  { value: 'Harley-Davidson', label: 'Harley-Davidson' },
  { value: 'Triumph', label: 'Triumph' },
  { value: 'Ducati', label: 'Ducati' },
];

const categoriaOptions = [
  { value: 'Street', label: 'Street' },
  { value: 'Trail', label: 'Trail' },
  { value: 'Scooter', label: 'Scooter' },
  { value: 'Custom', label: 'Custom' },
  { value: 'Sport', label: 'Sport' },
  { value: 'Adventure', label: 'Adventure' },
];

const currentYear = new Date().getFullYear();
const minModelYear = 1901;

export default function FormCatalogoModeloMoto({ onBack }: CatalogoMotosCreateProps) {
  const [form] = Form.useForm<ModeloMotoRequest>();
  const [linhas, setLinhas] = useState<Linha[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const fetchLinhas = async () => {
      try {
        setLoading(true);
        const linhasData = await linhaService.getAll(true);
        setLinhas(linhasData);
      } catch (error) {
        handleApiError(error, 'error.fetchLinhas');
      } finally {
        setLoading(false);
      }
    };

    fetchLinhas();
  }, []);

  const handleSubmit = async (values: ModeloMotoRequest) => {
    try {
      setLoading(true);
      await modeloMotoService.create(values);
      message.success(t('modeloMotoCreatedSuccess'));
      form.resetFields();
      onBack();
    } catch (error) {
      handleApiError(error);
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
      <Space orientation="vertical" size="large" style={{ width: '100%' }}>
        <Space orientation="vertical" size="middle" style={{ width: '100%' }}>
          <DashboardBreadcrumb
            userType="concessionaria"
            items={[
              {
                title: t('modeloMotoCatalog.title'),
                icon: <CarOutlined />,
                path: `/dashboard/concessionaria/${PATH_SEGMENTS.CONCESSIONARIA_CATALOGOS_MOTOS}`,
              }
              {
                title: t('modeloMotoCatalog.new'),
              },
            ]}
          />

          <Space align="center">
            <Button icon={<ArrowLeftOutlined />} onClick={handleCancel}>
              {t('back')}
            </Button>
            <Title level={2} style={{ margin: 0 }}>
              {t('modeloMotoCatalog.registerNew')}
            </Title>
          </Space>
        </Space>

        <Card>
          <Form form={form} layout="vertical" onFinish={handleSubmit}>
            <Form.Item
              label={t('modeloMotoCatalog.brand')}
              name="marca"
              rules={[{ required: true, message: t('modeloMotoCatalog.brandRequired') }]}
            >
              <Select
                placeholder={t('modeloMotoCatalog.brandPlaceholder')}
                options={marcaOptions}
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
              <Input placeholder="Ex: CG 160, MT-03, GSX-S750" />
            </Form.Item>

            <Form.Item
              label={t('modeloMotoCatalog.category')}
              name="categoria"
            >
              <Select
                allowClear
                placeholder={t('modeloMotoCatalog.categoryPlaceholder')}
                options={categoriaOptions}
              />
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
                parser={(value) => value?.replace(/[^\d]/g, '') as any}
              />
            </Form.Item>

            <Form.Item
              label={t('modeloMotoCatalog.engine')}
              name="cilindrada"
              rules={[{ max: 50, message: t('modeloMotoCatalog.engineLength') }]}
            >
              <Input placeholder="Ex: 160cc, 300cc, 750cc" />
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
              <Space style={{ width: '100%', justifyContent: 'flex-end' }}>
                <Button onClick={handleCancel}>
                  {t('modeloMotoCatalog.cancel')}
                </Button>
                <Button type="primary" htmlType="submit" loading={loading}>
                  {t('modeloMotoCatalog.save')}
                </Button>
              </Space>
            </Form.Item>
          </Form>
        </Card>
      </Space>
    </Spin>
  );
}
