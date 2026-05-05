import { Form, Input, Button, Tabs, Layout, Typography, Card, Space } from 'antd';
import { UserOutlined, LockOutlined, ShopOutlined, MailOutlined, PhoneOutlined, IdcardOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router';

const { Content } = Layout;
const { Title, Text, Link } = Typography;

export default function Cadastro() {
  const navigate = useNavigate();

  const handleClienteCadastro = (values: any) => {
    console.log('Cliente Cadastro:', values);
    navigate('/dashboard/cliente');
  };

  const handleConcessionariaCadastro = (values: any) => {
    console.log('Concessionária Cadastro:', values);
    navigate('/dashboard/concessionaria');
  };

  const clienteTab = (
    <Space direction="vertical" size="large" style={{ width: '100%' }}>
      <Form
        name="cliente_cadastro"
        onFinish={handleClienteCadastro}
        layout="vertical"
        size="large"
      >
        <Form.Item
          label="Nome Completo"
          name="nomeProprietario"
          rules={[{ required: true, message: 'Por favor, insira seu nome completo' }]}
        >
          <Input prefix={<UserOutlined />} placeholder="Nome completo" />
        </Form.Item>

        <Form.Item
          label="CPF"
          name="cpf"
          rules={[{ required: true, message: 'Por favor, insira seu CPF' }]}
        >
          <Input prefix={<IdcardOutlined />} placeholder="000.000.000-00" />
        </Form.Item>

        <Form.Item
          label="Email"
          name="email"
          rules={[
            { required: true, message: 'Por favor, insira seu email' },
            { type: 'email', message: 'Email inválido' }
          ]}
        >
          <Input prefix={<MailOutlined />} placeholder="seu@email.com" />
        </Form.Item>

        <Form.Item
          label="Celular"
          name="cel"
          rules={[{ required: true, message: 'Por favor, insira seu celular' }]}
        >
          <Input prefix={<PhoneOutlined />} placeholder="(00) 00000-0000" />
        </Form.Item>

        <Form.Item
          label="Senha"
          name="senha"
          rules={[
            { required: true, message: 'Por favor, insira uma senha' },
            { min: 6, message: 'A senha deve ter no mínimo 6 caracteres' }
          ]}
        >
          <Input.Password prefix={<LockOutlined />} placeholder="Senha" />
        </Form.Item>

        <Form.Item
          label="Confirmar Senha"
          name="confirmarSenha"
          dependencies={['senha']}
          rules={[
            { required: true, message: 'Por favor, confirme sua senha' },
            ({ getFieldValue }) => ({
              validator(_, value) {
                if (!value || getFieldValue('senha') === value) {
                  return Promise.resolve();
                }
                return Promise.reject(new Error('As senhas não coincidem'));
              },
            }),
          ]}
        >
          <Input.Password prefix={<LockOutlined />} placeholder="Confirme sua senha" />
        </Form.Item>

        <Form.Item>
          <Button type="primary" htmlType="submit" block size="large">
            Cadastrar como Cliente
          </Button>
        </Form.Item>
      </Form>
    </Space>
  );

  const concessionariaTab = (
    <Space direction="vertical" size="large" style={{ width: '100%' }}>
      <Form
        name="concessionaria_cadastro"
        onFinish={handleConcessionariaCadastro}
        layout="vertical"
        size="large"
      >
        <Form.Item
          label="Nome da Concessionária"
          name="nomeConcessionaria"
          rules={[{ required: true, message: 'Por favor, insira o nome da concessionária' }]}
        >
          <Input prefix={<ShopOutlined />} placeholder="Nome da concessionária" />
        </Form.Item>

        <Form.Item
          label="CNPJ"
          name="cnpj"
          rules={[{ required: true, message: 'Por favor, insira o CNPJ' }]}
        >
          <Input prefix={<IdcardOutlined />} placeholder="00.000.000/0000-00" />
        </Form.Item>

        <Form.Item
          label="Telefone"
          name="tel"
          rules={[{ required: true, message: 'Por favor, insira o telefone' }]}
        >
          <Input prefix={<PhoneOutlined />} placeholder="(00) 0000-0000" />
        </Form.Item>

        <Form.Item
          label="Senha"
          name="senha"
          rules={[
            { required: true, message: 'Por favor, insira uma senha' },
            { min: 6, message: 'A senha deve ter no mínimo 6 caracteres' }
          ]}
        >
          <Input.Password prefix={<LockOutlined />} placeholder="Senha" />
        </Form.Item>

        <Form.Item
          label="Confirmar Senha"
          name="confirmarSenha"
          dependencies={['senha']}
          rules={[
            { required: true, message: 'Por favor, confirme sua senha' },
            ({ getFieldValue }) => ({
              validator(_, value) {
                if (!value || getFieldValue('senha') === value) {
                  return Promise.resolve();
                }
                return Promise.reject(new Error('As senhas não coincidem'));
              },
            }),
          ]}
        >
          <Input.Password prefix={<LockOutlined />} placeholder="Confirme sua senha" />
        </Form.Item>

        <Form.Item>
          <Button type="primary" htmlType="submit" block size="large">
            Cadastrar Concessionária
          </Button>
        </Form.Item>
      </Form>
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
            <Text type="secondary">Crie sua conta para começar a gerenciar suas revisões</Text>
          </Space>

          <Card>
            <Tabs items={items} centered size="large" />
          </Card>

          <Space direction="vertical" size="small" style={{ width: '100%', textAlign: 'center' }}>
            <Text type="secondary">
              Já tem uma conta? <Link onClick={() => navigate('/login')}>Entrar</Link>
            </Text>
            <Link onClick={() => navigate('/')}>Voltar para página inicial</Link>
          </Space>
        </Space>
      </Content>
    </Layout>
  );
}
