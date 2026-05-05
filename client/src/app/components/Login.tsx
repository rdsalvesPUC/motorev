import { Form, Input, Button, Tabs, Layout, Typography, Card, Space, Divider, Flex } from 'antd';
import { UserOutlined, LockOutlined, ShopOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router';

const { Content } = Layout;
const { Title, Text, Link } = Typography;

export default function Login() {
  const navigate = useNavigate();

  const handleClienteLogin = (values: any) => {
    console.log('Cliente Login:', values);
    navigate('/dashboard/cliente');
  };

  const handleConcessionariaLogin = (values: any) => {
    console.log('Concessionária Login:', values);
    navigate('/dashboard/concessionaria');
  };

  const clienteTab = (
    <Space direction="vertical" size="large" style={{ width: '100%' }}>
      <Form
        name="cliente_login"
        onFinish={handleClienteLogin}
        layout="vertical"
        size="large"
      >
        <Form.Item
          label="CPF"
          name="cpf"
          rules={[{ required: true, message: 'Por favor, insira seu CPF' }]}
        >
          <Input prefix={<UserOutlined />} placeholder="000.000.000-00" />
        </Form.Item>

        <Form.Item
          label="Senha"
          name="senha"
          rules={[{ required: true, message: 'Por favor, insira sua senha' }]}
        >
          <Input.Password prefix={<LockOutlined />} placeholder="Senha" />
        </Form.Item>

        <Form.Item>
          <Flex justify="space-between" align="center">
            <Link>Esqueci minha senha</Link>
          </Flex>
        </Form.Item>

        <Form.Item>
          <Button type="primary" htmlType="submit" block size="large">
            Entrar como Cliente
          </Button>
        </Form.Item>
      </Form>

      <Space direction="vertical" size="small" style={{ width: '100%', textAlign: 'center' }}>
        <Text type="secondary">
          Não tem uma conta? <Link onClick={() => navigate('/cadastro')}>Cadastre-se</Link>
        </Text>
      </Space>
    </Space>
  );

  const concessionariaTab = (
    <Space direction="vertical" size="large" style={{ width: '100%' }}>
      <Form
        name="concessionaria_login"
        onFinish={handleConcessionariaLogin}
        layout="vertical"
        size="large"
      >
        <Form.Item
          label="CNPJ"
          name="cnpj"
          rules={[{ required: true, message: 'Por favor, insira o CNPJ' }]}
        >
          <Input prefix={<ShopOutlined />} placeholder="00.000.000/0000-00" />
        </Form.Item>

        <Form.Item
          label="Senha"
          name="senha"
          rules={[{ required: true, message: 'Por favor, insira sua senha' }]}
        >
          <Input.Password prefix={<LockOutlined />} placeholder="Senha" />
        </Form.Item>

        <Form.Item>
          <Flex justify="space-between" align="center">
            <Link>Esqueci minha senha</Link>
          </Flex>
        </Form.Item>

        <Form.Item>
          <Button type="primary" htmlType="submit" block size="large">
            Entrar como Concessionária
          </Button>
        </Form.Item>
      </Form>

      <Space direction="vertical" size="small" style={{ width: '100%', textAlign: 'center' }}>
        <Text type="secondary">
          Não tem uma conta? <Link onClick={() => navigate('/cadastro')}>Cadastre-se</Link>
        </Text>
      </Space>
    </Space>
  );

  const items = [
    {
      key: 'cliente',
      label: 'Cliente',
      children: clienteTab,
    },
    {
      key: 'concessionaria',
      label: 'Concessionária',
      children: concessionariaTab,
    },
  ];

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Content style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', padding: '24px' }}>
        <Space direction="vertical" size="large" style={{ width: '100%', maxWidth: '480px' }}>
          <Space direction="vertical" size="small" style={{ width: '100%', textAlign: 'center' }}>
            <Title level={2} style={{ margin: 0 }}>MotoRev</Title>
            <Text type="secondary">Entre na sua conta para gerenciar suas revisões</Text>
          </Space>

          <Card>
            <Tabs items={items} centered size="large" />
          </Card>

          <Space direction="vertical" size="small" style={{ width: '100%', textAlign: 'center' }}>
            <Link onClick={() => navigate('/')}>Voltar para página inicial</Link>
          </Space>
        </Space>
      </Content>
    </Layout>
  );
}
