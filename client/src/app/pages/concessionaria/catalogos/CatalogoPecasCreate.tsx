import { Breadcrumb, Typography, Form, Input, Select, Button, Space, Flex, InputNumber, Card, message } from 'antd';
import { HomeOutlined, ToolOutlined, ArrowLeftOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router';
import { CATEGORIAS_PECA, type CategoriaPeca, pecaService, type PecaRequest } from '@/app/services/pecaService';
import { PATHS } from '@/app/paths';
import { t } from '@/app/i18n';
import { getCurrencySymbol, getDecimalSeparator } from '@/app/utils/formatters';

const { Title } = Typography;

const categoriaPecaKeys: Record<CategoriaPeca, string> = {
  Filtros: 'partsCatalog.category.filters',
  Motor: 'partsCatalog.category.engine',
  Freios: 'partsCatalog.category.brakes',
  Transmissão: 'partsCatalog.category.transmission',
  Elétrica: 'partsCatalog.category.electrical',
};

function getCategoriaPecaOptions() {
  return CATEGORIAS_PECA.map(({ value }) => ({
    value,
    label: t(categoriaPecaKeys[value]),
  }));
}

export default function CatalogoPecasCreate() {
  const [form] = Form.useForm<PecaRequest>();
  const navigate = useNavigate();

  const goBack = () => {
    navigate(PATHS.CONCESSIONARIA_CATALOGOS_PECAS);
  };

  const handleSubmit = async (values: PecaRequest) => {
    try {
      await pecaService.criar({
        ...values,
        preco: Number(values.preco),
        estoque: Number(values.estoque),
      });

      message.success(t('partsCatalog.create.success'));
      goBack();
    } catch (err: any) {
      message.error(err.message || t('partsCatalog.create.error'));
    }
  };

  const handleCancel = () => {
    form.resetFields();
    goBack();
  };

  return (
    <Space direction="vertical" size="large" style={{ width: '100%' }}>
      <Space direction="vertical" size="middle" style={{ width: '100%' }}>
        <Breadcrumb
          items={[
            {
              title: <HomeOutlined />,
            },
            {
              title: (
                <>
                  <ToolOutlined />
                  <span>{t('dashboard.menu.catalogos')}</span>
                </>
              ),
            },
            {
              title: t('partsCatalog.title'),
            },
            {
              title: t('partsCatalog.addPart'),
            },
          ]}
        />

        <Flex align="center" gap="middle">
          <Button type="default" icon={<ArrowLeftOutlined />} onClick={goBack} />
          <Title level={2} style={{ margin: 0 }}>
            {t('partsCatalog.addPart')}
          </Title>
        </Flex>
      </Space>

      <Card>
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
          style={{ maxWidth: '800px' }}
        >
          <Form.Item
            label={t('partsCatalog.code')}
            name="codigo"
            rules={[
              { required: true, message: t('partsCatalog.code.required') },
              { min: 2, max: 20, message: t('partsCatalog.code.length') },
            ]}
          >
            <Input placeholder={t('partsCatalog.code.placeholder')} />
          </Form.Item>

          <Form.Item
            label={t('partsCatalog.partName')}
            name="nome"
            rules={[
              { required: true, message: t('partsCatalog.partName.required') },
              { min: 3, max: 150, message: t('partsCatalog.partName.length') },
            ]}
          >
            <Input placeholder={t('partsCatalog.partName.placeholder')} />
          </Form.Item>

          <Form.Item
            label={t('partsCatalog.category')}
            name="categoria"
            rules={[{ required: true, message: t('partsCatalog.category.required') }]}
          >
            <Select placeholder={t('partsCatalog.category.placeholder')} options={getCategoriaPecaOptions()} />
          </Form.Item>

          <Form.Item
            label={t('partsCatalog.price')}
            name="preco"
            rules={[{ required: true, message: t('partsCatalog.price.required') }]}
          >
            <InputNumber
              placeholder={t('partsCatalog.price.placeholder')}
              addonBefore={getCurrencySymbol()}
              min={0.01}
              precision={2}
              step={0.01}
              decimalSeparator={getDecimalSeparator()}
              style={{ width: '100%' }}
            />
          </Form.Item>

          <Form.Item
            label={t('partsCatalog.stock')}
            name="estoque"
            rules={[{ required: true, message: t('partsCatalog.stock.required') }]}
          >
            <InputNumber placeholder={t('partsCatalog.stock.placeholder')} style={{ width: '100%' }} min={0} precision={0} />
          </Form.Item>

          <Form.Item>
            <Flex gap="middle">
              <Button type="primary" htmlType="submit" size="large">
                {t('partsCatalog.save')}
              </Button>
              <Button size="large" onClick={handleCancel}>
                {t('partsCatalog.cancel')}
              </Button>
            </Flex>
          </Form.Item>
        </Form>
      </Card>
    </Space>
  );
}
