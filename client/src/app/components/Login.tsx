import { Form, Input, Button, Layout, Typography, Card, Space, Flex, message } from 'antd';
import { MailOutlined, LockOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router';
import { useState, useEffect } from 'react';
import { authService } from '../services/authService';
import { t } from '../i18n';
import LanguageSelector from './LanguageSelector';

const { Content } = Layout;
const { Title, Text, Link } = Typography;

export default function Login() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [, forceUpdate] = useState({});

  useEffect(() => {
    const handleLangChange = () => forceUpdate({});
    window.addEventListener('languagechange', handleLangChange);
    return () => window.removeEventListener('languagechange', handleLangChange);
  }, []);

  const handleLogin = async (values: any) => {
    try {
      setLoading(true);
      const data = await authService.login({ email: values.email, password: values.senha });
      
      localStorage.setItem('token', data.token);
      localStorage.setItem('refreshToken', data.refreshToken);
      localStorage.setItem('user', JSON.stringify(data.usuario));
      localStorage.setItem('perfil', data.perfil);

      message.success(t('login.success'));
      
      if (data.perfil === 'Cliente') {
        navigate('/dashboard/cliente');
      } else {
        navigate('/dashboard/concessionaria');
      }
    } catch (error: any) {
      if (error.status === 400 || error.status === 401 || error.status === 404) {
        message.error(t('login.error.invalidCredentials'));
      } else {
        message.error(t('login.error'));
      }
    } finally {
      setLoading(false);
    }
  };

  const loginForm = (
    <Space direction="vertical" size="large" style={{ width: '100%' }}>
      <Form
        name="login"
        onFinish={handleLogin}
        layout="vertical"
        size="large"
      >
        <Form.Item
          label={t('login.email.label')}
          name="email"
          rules={[
            { required: true, message: t('login.email.required') },
            { type: 'email', message: t('login.email.invalid') }
          ]}
        >
          <Input prefix={<MailOutlined />} placeholder={t('login.email.placeholder')} />
        </Form.Item>

        <Form.Item
          label={t('login.password.label')}
          name="senha"
          rules={[{ required: true, message: t('login.password.required') }]}
        >
          <Input.Password prefix={<LockOutlined />} placeholder={t('login.password.placeholder')} />
        </Form.Item>

        <Form.Item>
          <Flex justify="space-between" align="center">
            <Link>{t('login.forgotPassword')}</Link>
          </Flex>
        </Form.Item>

        <Form.Item>
          <Button type="primary" htmlType="submit" block size="large" loading={loading}>
            {t('login.submit')}
          </Button>
        </Form.Item>
      </Form>

      <Space direction="vertical" size="small" style={{ width: '100%', textAlign: 'center' }}>
        <Text type="secondary">
          {t('login.noAccount')} <Link onClick={() => navigate('/cadastro')}>{t('login.signUp')}</Link>
        </Text>
      </Space>
    </Space>
  );

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Content style={{ display: 'flex', flexDirection: 'column', justifyContent: 'center', alignItems: 'center', padding: '24px' }}>
        <div style={{ position: 'absolute', top: 24, right: 24 }}>
          <LanguageSelector />
        </div>

        <Space direction="vertical" size="large" style={{ width: '100%', maxWidth: '480px' }}>
          <Space direction="vertical" size="small" style={{ width: '100%', textAlign: 'center' }}>
            <Title level={2} style={{ margin: 0 }}>{t('login.title')}</Title>
            <Text type="secondary">{t('login.subtitle')}</Text>
          </Space>

          <Card>
            {loginForm}
          </Card>

          <Space direction="vertical" size="small" style={{ width: '100%', textAlign: 'center' }}>
            <Link onClick={() => navigate('/')}>{t('login.back')}</Link>
          </Space>
        </Space>
      </Content>
    </Layout>
  );
}
