import { Form, Input, Button, Tabs, Layout, Typography, Card, Space, message } from 'antd';
import { UserOutlined, LockOutlined, ShopOutlined, MailOutlined, IdcardOutlined, PhoneOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router';
import { useState, useEffect } from 'react';
import { clienteService } from '../services/clienteService';
import { concessionariaService } from '../services/concessionariaService';
import { t } from '../i18n';
import LanguageSelector from './LanguageSelector';

const { Content } = Layout;
const { Title, Text, Link } = Typography;

export default function Cadastro() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [, forceUpdate] = useState({});

  useEffect(() => {
    const handleLangChange = () => forceUpdate({});
    window.addEventListener('languagechange', handleLangChange);
    return () => window.removeEventListener('languagechange', handleLangChange);
  }, []);

  const handleClienteCadastro = async (values: any) => {
    try {
      setLoading(true);
      await clienteService.register({
        nome: values.nomeProprietario,
        email: values.email,
        password: values.senha
      });
      message.success(t('cadastro.cliente.success'));
      navigate('/login');
    } catch (error: any) {
      if (error.status === 409) {
        message.error(t('cadastro.error.conflict'));
      } else if (error.status === 400) {
        message.error(t('cadastro.error.validation'));
      } else {
        message.error(t('cadastro.error'));
      }
    } finally {
      setLoading(false);
    }
  };

  const handleConcessionariaCadastro = async (values: any) => {
    try {
      setLoading(true);
      await concessionariaService.register({
        nome: values.nomeConcessionaria,
        email: values.email,
        password: values.senha
      });
      message.success(t('cadastro.concessionaria.success'));
      navigate('/login');
    } catch (error: any) {
      if (error.status === 409) {
        message.error(t('cadastro.error.conflict'));
      } else if (error.status === 400) {
        message.error(t('cadastro.error.validation'));
      } else {
        message.error(t('cadastro.error'));
      }
    } finally {
      setLoading(false);
    }
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
          label={t('cadastro.cliente.nome.label')}
          name="nomeProprietario"
          rules={[{ required: true, message: t('cadastro.cliente.nome.required') }]}
        >
          <Input prefix={<UserOutlined />} placeholder={t('cadastro.cliente.nome.placeholder')} />
        </Form.Item>

        <Form.Item
          label={t('cadastro.cliente.cpf.label')}
          name="cpf"
          rules={[{ required: true, message: t('cadastro.cliente.cpf.required') }]}
        >
          <Input prefix={<IdcardOutlined />} placeholder={t('cadastro.cliente.cpf.placeholder')} />
        </Form.Item>

        <Form.Item
          label={t('cadastro.email.label')}
          name="email"
          rules={[
            { required: true, message: t('cadastro.email.required') },
            { type: 'email', message: t('cadastro.email.invalid') }
          ]}
        >
          <Input prefix={<MailOutlined />} placeholder={t('cadastro.email.placeholder')} />
        </Form.Item>

        <Form.Item
          label={t('cadastro.celular.label')}
          name="cel"
          rules={[{ required: true, message: t('cadastro.celular.required') }]}
        >
          <Input prefix={<PhoneOutlined />} placeholder={t('cadastro.celular.placeholder')} />
        </Form.Item>

        <Form.Item
          label={t('cadastro.senha.label')}
          name="senha"
          rules={[
            { required: true, message: t('cadastro.senha.required') },
            { min: 6, message: t('cadastro.senha.min') }
          ]}
        >
          <Input.Password prefix={<LockOutlined />} placeholder={t('cadastro.senha.placeholder')} />
        </Form.Item>

        <Form.Item
          label={t('cadastro.confirmarSenha.label')}
          name="confirmarSenha"
          dependencies={['senha']}
          rules={[
            { required: true, message: t('cadastro.confirmarSenha.required') },
            ({ getFieldValue }) => ({
              validator(_, value) {
                if (!value || getFieldValue('senha') === value) {
                  return Promise.resolve();
                }
                return Promise.reject(new Error(t('cadastro.confirmarSenha.match')));
              },
            }),
          ]}
        >
          <Input.Password prefix={<LockOutlined />} placeholder={t('cadastro.confirmarSenha.placeholder')} />
        </Form.Item>

        <Form.Item>
          <Button type="primary" htmlType="submit" block size="large" loading={loading}>
            {t('cadastro.cliente.submit')}
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
          label={t('cadastro.concessionaria.nome.label')}
          name="nomeConcessionaria"
          rules={[{ required: true, message: t('cadastro.concessionaria.nome.required') }]}
        >
          <Input prefix={<ShopOutlined />} placeholder={t('cadastro.concessionaria.nome.placeholder')} />
        </Form.Item>

        <Form.Item
          label={t('cadastro.concessionaria.cnpj.label')}
          name="cnpj"
          rules={[{ required: true, message: t('cadastro.concessionaria.cnpj.required') }]}
        >
          <Input prefix={<IdcardOutlined />} placeholder={t('cadastro.concessionaria.cnpj.placeholder')} />
        </Form.Item>

        <Form.Item
          label={t('cadastro.concessionaria.telefone.label')}
          name="tel"
          rules={[{ required: true, message: t('cadastro.concessionaria.telefone.required') }]}
        >
          <Input prefix={<PhoneOutlined />} placeholder={t('cadastro.concessionaria.telefone.placeholder')} />
        </Form.Item>

        <Form.Item
          label={t('cadastro.email.label')}
          name="email"
          rules={[
            { required: true, message: t('cadastro.email.required') },
            { type: 'email', message: t('cadastro.email.invalid') }
          ]}
        >
          <Input prefix={<MailOutlined />} placeholder={t('cadastro.concessionaria.email.placeholder')} />
        </Form.Item>

        <Form.Item
          label={t('cadastro.senha.label')}
          name="senha"
          rules={[
            { required: true, message: t('cadastro.senha.required') },
            { min: 6, message: t('cadastro.senha.min') }
          ]}
        >
          <Input.Password prefix={<LockOutlined />} placeholder={t('cadastro.senha.placeholder')} />
        </Form.Item>

        <Form.Item
          label={t('cadastro.confirmarSenha.label')}
          name="confirmarSenha"
          dependencies={['senha']}
          rules={[
            { required: true, message: t('cadastro.confirmarSenha.required') },
            ({ getFieldValue }) => ({
              validator(_, value) {
                if (!value || getFieldValue('senha') === value) {
                  return Promise.resolve();
                }
                return Promise.reject(new Error(t('cadastro.confirmarSenha.match')));
              },
            }),
          ]}
        >
          <Input.Password prefix={<LockOutlined />} placeholder={t('cadastro.confirmarSenha.placeholder')} />
        </Form.Item>

        <Form.Item>
          <Button type="primary" htmlType="submit" block size="large" loading={loading}>
            {t('cadastro.concessionaria.submit')}
          </Button>
        </Form.Item>
      </Form>
    </Space>
  );

  const items = [
    {
      key: 'cliente',
      label: t('cadastro.cliente.tab'),
      children: clienteTab,
    },
    {
      key: 'concessionaria',
      label: t('cadastro.concessionaria.tab'),
      children: concessionariaTab,
    },
  ];

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Content style={{ display: 'flex', flexDirection: 'column', justifyContent: 'center', alignItems: 'center', padding: '24px' }}>
        <div style={{ position: 'absolute', top: 24, right: 24 }}>
          <LanguageSelector />
        </div>

        <Space direction="vertical" size="large" style={{ width: '100%', maxWidth: '480px' }}>
          <Space direction="vertical" size="small" style={{ width: '100%', textAlign: 'center' }}>
            <Title level={2} style={{ margin: 0 }}>{t('cadastro.title')}</Title>
            <Text type="secondary">{t('cadastro.subtitle')}</Text>
          </Space>

          <Card>
            <Tabs items={items} centered size="large" />
          </Card>

          <Space direction="vertical" size="small" style={{ width: '100%', textAlign: 'center' }}>
            <Text type="secondary">
              {t('cadastro.hasAccount')} <Link onClick={() => navigate('/login')}>{t('cadastro.login')}</Link>
            </Text>
            <Link onClick={() => navigate('/')}>{t('cadastro.back')}</Link>
          </Space>
        </Space>
      </Content>
    </Layout>
  );
}
