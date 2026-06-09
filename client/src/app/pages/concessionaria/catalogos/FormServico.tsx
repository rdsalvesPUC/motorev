import { Typography, Form, Input, InputNumber, Select, Button, Flex, message, Spin, theme } from 'antd';
import { ToolOutlined, ArrowLeftOutlined } from '@ant-design/icons';
import DashboardBreadcrumb from '@/app/components/layout/DashboardBreadcrumb';
import { useForm, Controller } from 'react-hook-form';
import { servicoService } from '@/app/services/servicoService';
import { ServicoRequest } from '@/app/models/ServicoRequest';
import { handleApiError } from '@/app/utils/errorHandler';
import { t } from '@/app/i18n';
import {
  formatCurrencyInput,
  getCurrencySymbol,
  getDecimalSeparator,
  parseCurrencyInput,
} from '@/app/utils/formatters';
import { useState } from 'react';
import { PATH_SEGMENTS } from '@/app/paths';

const { Title } = Typography;

interface FormServicoProps {
  onCancel: () => void;
}

export default function FormServico({ onCancel }: FormServicoProps) {
  const { token } = theme.useToken();
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
      onCancel();
    } catch (error) {
      handleApiError(error, 'error.createService');
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
                title: t('serviceCatalog.title'),
                path: PATH_SEGMENTS.CONCESSIONARIA_CATALOGOS_SERVICOS,
              },
              {
                title: t('serviceCatalog.newService'),
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
              {t('serviceCatalog.registerNewService')}
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
            layout="vertical"
            onFinish={handleSubmit(handleFormSubmit)}
            style={{ maxWidth: '800px' }}
          >
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
                    placeholder={t('serviceCatalog.codePlaceholder')}
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
                  <Input {...field} placeholder={t('serviceCatalog.namePlaceholder')} />
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
                    placeholder={t('serviceCatalog.estimatedTimePlaceholder')}
                    min={1}
                    max={480}
                    keyboard={true}
                    stringMode={false}
                    parser={(value) => value?.replace(/[^\d]/g, '') as any}
                  />
                )}
              />
            </Form.Item>

            <Form.Item
              label={t('serviceCatalog.priceBRL', { currencySymbol: getCurrencySymbol() })}
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
                    placeholder={t('serviceCatalog.pricePlaceholder')}
                    min={0}
                    max={100000}
                    step={1}
                    prefix={getCurrencySymbol()}
                    precision={2}
                    decimalSeparator={getDecimalSeparator()}
                    parser={(value) => parseCurrencyInput(value) as any}
                    formatter={formatCurrencyInput}
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
                  />
                )}
              />
            </Form.Item>

            <Form.Item style={{ marginBottom: 0 }}>
              <Flex gap="middle">
                <Button type="primary" htmlType="submit" loading={loading}>
                  {t('serviceCatalog.save')}
                </Button>
                <Button onClick={handleCancel}>
                  {t('serviceCatalog.cancel')}
                </Button>
              </Flex>
            </Form.Item>
          </Form>
        </div>
      </Flex>
    </Spin>
  );
}
