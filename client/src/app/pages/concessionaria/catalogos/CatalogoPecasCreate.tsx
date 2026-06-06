import { Breadcrumb, Typography, Form, Input, Select, Button, Space, Flex, InputNumber, Card, message } from 'antd';
import { HomeOutlined, ToolOutlined, ArrowLeftOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router';
import { CATEGORIAS_PECA, pecaService, type PecaRequest } from '@/app/services/pecaService';
import { PATHS } from '@/app/paths';

const { Title } = Typography;

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

      message.success('Peça cadastrada com sucesso.');
      goBack();
    } catch (err: any) {
      message.error(err.message || 'Não foi possível cadastrar a peça.');
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
                  <span>Catálogos</span>
                </>
              ),
            },
            {
              title: 'Peças',
            },
            {
              title: 'Adicionar Peça',
            },
          ]}
        />

        <Flex align="center" gap="middle">
          <Button type="default" icon={<ArrowLeftOutlined />} onClick={goBack} />
          <Title level={2} style={{ margin: 0 }}>
            Adicionar Peça
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
            label="Código"
            name="codigo"
            rules={[
              { required: true, message: 'Por favor, insira o código da peça' },
              { min: 2, max: 20, message: 'O código deve ter entre 2 e 20 caracteres' },
            ]}
          >
            <Input placeholder="Ex: P001, P002" />
          </Form.Item>

          <Form.Item
            label="Nome da Peça"
            name="nome"
            rules={[
              { required: true, message: 'Por favor, insira o nome da peça' },
              { min: 3, max: 150, message: 'O nome deve ter entre 3 e 150 caracteres' },
            ]}
          >
            <Input placeholder="Ex: Filtro de Óleo, Vela de Ignição" />
          </Form.Item>

          <Form.Item
            label="Categoria"
            name="categoria"
            rules={[{ required: true, message: 'Por favor, selecione a categoria' }]}
          >
            <Select placeholder="Selecione a categoria" options={CATEGORIAS_PECA} />
          </Form.Item>

          <Form.Item
            label="Preço"
            name="preco"
            rules={[{ required: true, message: 'Por favor, insira o preço' }]}
          >
            <InputNumber
              placeholder="Ex: 35,00"
              addonBefore="R$"
              min={0.01}
              precision={2}
              step={0.01}
              decimalSeparator=","
              style={{ width: '100%' }}
            />
          </Form.Item>

          <Form.Item
            label="Estoque"
            name="estoque"
            rules={[{ required: true, message: 'Por favor, insira a quantidade em estoque' }]}
          >
            <InputNumber placeholder="Ex: 25, 50" style={{ width: '100%' }} min={0} precision={0} />
          </Form.Item>

          <Form.Item>
            <Flex gap="middle">
              <Button type="primary" htmlType="submit" size="large">
                Salvar
              </Button>
              <Button size="large" onClick={handleCancel}>
                Cancelar
              </Button>
            </Flex>
          </Form.Item>
        </Form>
      </Card>
    </Space>
  );
}
