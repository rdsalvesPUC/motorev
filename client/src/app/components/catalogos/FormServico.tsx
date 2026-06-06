import { Typography, Form, Input, InputNumber, Select, Button, Space, message, Card, Spin } from 'antd';
import { ToolOutlined, ArrowLeftOutlined } from '@ant-design/icons';
import DashboardBreadcrumb from '../common/DashboardBreadcrumb';
import { useForm, Controller } from 'react-hook-form';
import { servicoService } from '../../services/servicoService';
import { ServicoRequest } from '../../models/ServicoRequest';
import { ApiError } from '../../services/http';
import { handleApiError } from '../../utils/errorHandler';
import { t } from '../../i18n';
import { useState } from 'react';

const { Title } = Typography;

interface FormServicoProps {
  onCancel: () => void;
}

export default function FormServico({ onCancel }: FormServicoProps) {
  const { control, handleSubmit, reset, formState: { errors } } = useForm<ServicoRequest>({
    defaultValues: {
      codigo: '',
      nome: '',
      categoria: '',
      tempoEstimado: 0,
      custo: 0,
      descricao: '',
    }
  });
  const [loading, setLoading] = useState(false);

  const handleFormSubmit = async (data: ServicoRequest) => {
    try {
      setLoading(true);
      await servicoService.create(data);
      message.success(t('serviceCreatedSuccess'));
      reset();
      onCancel(); // Navigate back to the list
    } catch (error) {
      handleApiError(error);
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = () => {
    reset();
    onCancel();
  };

  return (
    <Spin spinning={loading}>
      <Space direction="vertical" size="large" style={{ width: '100%' }}>
        <Space direction="vertical" size="middle" style={{ width: '100%' }}>
          <DashboardBreadcrumb
            userType="concessionaria"
            items={[
              {
                title: t('serviceCatalog.title'),
                icon: <ToolOutlined />,
                path: 'catalogos-servicos',
              },
              {
                title: t('serviceCatalog.newService'),
              },
            ]}
          />

          <Space align="center">
            <Button icon={<ArrowLeftOutlined />} onClick={handleCancel}>
              {t('back')}
            </Button>
            <Title level={2} style={{ margin: 0 }}>
              {t('serviceCatalog.registerNewService')}
            </Title>
          </Space>
        </Space>

        <Card>
          <Form layout="vertical" onFinish={handleSubmit(handleFormSubmit)}>
            <Form.Item
              label={t('serviceCatalog.serviceCode')}
              validateStatus={errors.codigo ? 'error' : ''}
              help={errors.codigo?.message}
              required
            >
              <Controller
                name="codigo"
                control={control}
                rules={{
                  required: t('serviceCatalog.codeRequired'),
                  pattern: {
                    value: /^[A-Z0-9-]{3,20}$/,
                    message: t('serviceCatalog.codeInvalid')
                  }
                }}
                render={({ field }) => (
                  <Input
                    {...field}
                    placeholder="Ex: SERV001"
                    size="large"
                    style={{ textTransform: 'uppercase' }}
                    onChange={(e) => field.onChange(e.target.value.toUpperCase())}
                  />
                )}
              />
            </Form.Item>

            <Form.Item
              label={t('serviceCatalog.serviceName')}
              validateStatus={errors.nome ? 'error' : ''}
              help={errors.nome?.message}
              required
            >
              <Controller
                name="nome"
                control={control}
                rules={{
                  required: t('serviceCatalog.nameRequired'),
                  minLength: { value: 3, message: t('serviceCatalog.nameLength') },
                  maxLength: { value: 100, message: t('serviceCatalog.nameLength') }
                }}
                render={({ field }) => (
                  <Input {...field} placeholder="Ex: Troca de Óleo" size="large" />
                )}
              />
            </Form.Item>

            <Form.Item
              label={t('serviceCatalog.category')}
              validateStatus={errors.categoria ? 'error' : ''}
              help={errors.categoria?.message}
              required
            >
              <Controller
                name="categoria"
                control={control}
                rules={{ required: t('serviceCatalog.categoryRequired') }}
                render={({ field }) => (
                  <Select
                    {...field}
                    placeholder={t('serviceCatalog.category.placeholder')}
                    size="large"
                    options={[
                      { value: 'Verificacao', label: t('serviceCatalog.category.verificacao') },
                      { value: 'Ajuste', label: t('serviceCatalog.category.ajuste') },
                      { value: 'Limpeza', label: t('serviceCatalog.category.limpeza') },
                      { value: 'Troca', label: t('serviceCatalog.category.troca') },
                    ]}
                  />
                )}
              />
            </Form.Item>

            <Form.Item
              label={t('serviceCatalog.estimatedTimeMinutes')}
              validateStatus={errors.tempoEstimado ? 'error' : ''}
              help={errors.tempoEstimado?.message}
              required
            >
              <Controller
                name="tempoEstimado"
                control={control}
                rules={{
                  required: t('serviceCatalog.estimatedTimeRequired'),
                  min: { value: 1, message: t('serviceCatalog.estimatedTimeRange') },
                  max: { value: 480, message: t('serviceCatalog.estimatedTimeRange') }
                }}
                render={({ field }) => (
                  <InputNumber
                    {...field}
                    style={{ width: '100%' }}
                    placeholder="30"
                    min={1}
                    max={480}
                    size="large"
                    keyboard={true}
                    stringMode={false}
                    parser={(value) => value?.replace(/[^\d]/g, '') as any}
                  />
                )}
              />
            </Form.Item>

            <Form.Item
              label={t('serviceCatalog.priceBRL')}
              validateStatus={errors.custo ? 'error' : ''}
              help={errors.custo?.message}
              required
            >
              <Controller
                name="custo"
                control={control}
                rules={{
                  required: t('serviceCatalog.priceRequired'),
                  min: { value: 0, message: t('serviceCatalog.priceRange') },
                  max: { value: 100000, message: t('serviceCatalog.priceRange') }
                }}
                render={({ field }) => (
                  <InputNumber
                    {...field}
                    style={{ width: '100%' }}
                    placeholder="80.00"
                    min={0}
                    max={100000}
                    step={1}
                    prefix="R$"
                    precision={2}
                    size="large"
                    decimalSeparator=","
                    parser={(value) => value?.replace(/[^\d,]/g, '').replace(',', '.') as any}
                    formatter={(value) =>
                      value ? `${value}`.replace('.', ',').replace(/\B(?=(\d{3})+(?!\d))/g, '.') : ''
                    }
                  />
                )}
              />
            </Form.Item>

            <Form.Item
              label={t('serviceCatalog.description')}
              validateStatus={errors.descricao ? 'error' : ''}
              help={errors.descricao?.message}
              required
            >
              <Controller
                name="descricao"
                control={control}
                rules={{
                  required: t('serviceCatalog.descriptionRequired'),
                  maxLength: { value: 500, message: t('serviceCatalog.descriptionLength') }
                }}
                render={({ field }) => (
                  <Input.TextArea
                    {...field}
                    rows={4}
                    placeholder={t('serviceCatalog.descriptionPlaceholder')}
                    maxLength={500}
                    showCount
                    size="large"
                  />
                )}
              />
            </Form.Item>

            <Form.Item style={{ marginBottom: 0 }}>
              <Space style={{ width: '100%', justifyContent: 'flex-end' }}>
                <Button size="large" onClick={handleCancel}>
                  {t('serviceCatalog.cancel')}
                </Button>
                <Button type="primary" size="large" htmlType="submit" loading={loading}>
                  {t('serviceCatalog.save')}
                </Button>
              </Space>
            </Form.Item>
          </Form>
        </Card>
      </Space>
    </Spin>
  );
}
