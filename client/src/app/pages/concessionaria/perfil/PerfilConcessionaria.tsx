import { useEffect, useMemo, useState } from 'react';
import {
  Avatar,
  Breadcrumb,
  Button,
  Card,
  Descriptions,
  Divider,
  Flex,
  Form,
  Input,
  Modal,
  Radio,
  Skeleton,
  Space,
  Switch,
  Typography,
  message,
} from 'antd';
import {
  BulbOutlined,
  CloseOutlined,
  EditOutlined,
  EnvironmentOutlined,
  GlobalOutlined,
  HomeOutlined,
  IdcardOutlined,
  LockOutlined,
  MailOutlined,
  PhoneOutlined,
  SaveOutlined,
  SettingOutlined,
  ShopOutlined,
  UserOutlined,
} from '@ant-design/icons';
import { concessionariaService } from '@/app/services/concessionariaService';
import { viaCepService } from '@/app/services/viaCepService';
import { tokenManager } from '@/app/services/tokenManager';
import { Concessionaria } from '@/app/models/Concessionaria';
import { ConcessionariaPerfilRequest } from '@/app/models/ConcessionariaPerfilRequest';
import { useConfiguracoes, type Idioma } from '@/app/contexts/ConfiguracoesContext';
import { t } from '@/app/i18n';
import { formatCEP, formatCNPJ, formatPhone } from '@/app/utils/formatters';
import { CEP_REGEX, PHONE_REGEX, UF_REGEX } from '@/app/utils/validators';

const { Title, Text } = Typography;

interface SectionCardProps {
  title: string;
  icon: React.ReactNode;
  editing: boolean;
  saving?: boolean;
  onEdit?: () => void;
  onSave?: () => void;
  onCancel?: () => void;
  children: React.ReactNode;
}

function SectionCard({
  title,
  icon,
  editing,
  saving,
  onEdit,
  onSave,
  onCancel,
  children,
}: SectionCardProps) {
  return (
    <Card
      title={
        <Flex align="center" gap={8}>
          {icon}
          <span>{title}</span>
        </Flex>
      }
      extra={
        onEdit && (editing ? (
          <Flex gap={8} wrap="wrap">
            <Button size="small" icon={<CloseOutlined />} onClick={onCancel} disabled={saving}>
              {t('perfil.actions.cancel')}
            </Button>
            <Button size="small" type="primary" icon={<SaveOutlined />} onClick={onSave} loading={saving}>
              {t('perfil.actions.save')}
            </Button>
          </Flex>
        ) : (
          <Button size="small" icon={<EditOutlined />} onClick={onEdit}>
            {t('perfil.actions.edit')}
          </Button>
        ))
      }
    >
      {children}
    </Card>
  );
}

function normalizeText(value?: string | null) {
  return value?.trim() || '';
}

function buildPerfilPayload(values: ConcessionariaPerfilRequest): ConcessionariaPerfilRequest {
  return {
    nome: normalizeText(values.nome),
    email: normalizeText(values.email),
    cnpj: normalizeText(values.cnpj),
    telefone: normalizeText(values.telefone),
    cep: normalizeText(values.cep),
    logradouro: normalizeText(values.logradouro),
    numero: normalizeText(values.numero),
    bairro: normalizeText(values.bairro),
    cidade: normalizeText(values.cidade),
    uf: normalizeText(values.uf).toUpperCase(),
  };
}

export default function PerfilConcessionaria() {
  const [profile, setProfile] = useState<Concessionaria | null>(null);
  const [loading, setLoading] = useState(true);
  const [editingDados, setEditingDados] = useState(false);
  const [savingDados, setSavingDados] = useState(false);
  const [savingSenha, setSavingSenha] = useState(false);
  const [senhaModalOpen, setSenhaModalOpen] = useState(false);
  const { configuracoes, setIdioma, setTema } = useConfiguracoes();

  const [formDados] = Form.useForm<ConcessionariaPerfilRequest>();
  const [formSenha] = Form.useForm();

  const enderecoDescription = useMemo(() => {
    if (!profile) return null;
    return {
      linha1: [profile.logradouro, profile.numero].filter(Boolean).join(', '),
      bairro: normalizeText(profile.bairro),
      cidadeUf: [profile.cidade, profile.uf].filter(Boolean).join(' - '),
      cep: normalizeText(profile.cep),
    };
  }, [profile]);

  useEffect(() => {
    let mounted = true;

    const loadPerfil = async () => {
      try {
        const data = await concessionariaService.getMe();
        if (!mounted) return;
        setProfile(data);
      } catch (error: any) {
        if (mounted) {
          message.error(error.message || t('perfil.concessionaria.loading.error'));
        }
      } finally {
        if (mounted) {
          setLoading(false);
        }
      }
    };

    loadPerfil();

    return () => {
      mounted = false;
    };
  }, []);

  const updateProfile = (updatedProfile: Concessionaria) => {
    setProfile(updatedProfile);
    tokenManager.updateUserData({ nome: updatedProfile.nome, email: updatedProfile.email });
  };

  const buildCurrentPayload = (): ConcessionariaPerfilRequest | null => {
    if (!profile) return null;
    return {
      nome: profile.nome,
      email: profile.email,
      cnpj: profile.cnpj,
      telefone: profile.telefone,
      cep: profile.cep,
      logradouro: profile.logradouro,
      numero: profile.numero,
      bairro: profile.bairro,
      cidade: profile.cidade,
      uf: profile.uf,
    };
  };

  const handleEditDados = () => {
    if (!profile) return;
    formDados.setFieldsValue({
      nome: profile.nome,
      email: profile.email,
      cnpj: formatCNPJ(profile.cnpj),
      telefone: formatPhone(profile.telefone),
      cep: profile.cep,
      logradouro: profile.logradouro,
      numero: profile.numero,
      bairro: profile.bairro,
      cidade: profile.cidade,
      uf: profile.uf,
    });
    setEditingDados(true);
  };

  const handleSaveDados = async () => {
    const currentPayload = buildCurrentPayload();
    if (!currentPayload) return;

    try {
      const values = await formDados.validateFields();
      setSavingDados(true);
      const updatedProfile = await concessionariaService.updateMe(
        buildPerfilPayload({
          ...currentPayload,
          nome: values.nome,
          email: values.email,
          telefone: values.telefone,
        })
      );
      updateProfile(updatedProfile);
      setEditingDados(false);
      message.success(t('perfil.concessionaria.dados.updated'));
    } catch (error: any) {
      if (error?.errorFields) return;
      message.error(error.message || t('perfil.concessionaria.dados.updateError'));
    } finally {
      setSavingDados(false);
    }
  };



  const handleAlterarSenha = async () => {
    try {
      const values = await formSenha.validateFields();
      setSavingSenha(true);
      await concessionariaService.alterarSenha({
        senhaAtual: values.senhaAtual,
        novaSenha: values.novaSenha,
        confirmarNovaSenha: values.confirmarNovaSenha,
      });
      setSenhaModalOpen(false);
      formSenha.resetFields();
      message.success(t('perfil.senha.updated'));
    } catch (error: any) {
      if (error?.errorFields) return;
      message.error(error.message || t('perfil.senha.updateError'));
    } finally {
      setSavingSenha(false);
    }
  };

  const handleIdiomaChange = (nextIdioma: Idioma) => {
    setIdioma(nextIdioma);
  };

  const handleTemaChange = (checked: boolean) => {
    setTema(checked ? 'dark' : 'light');
  };

  if (loading) {
    return (
      <Card>
        <Skeleton active avatar paragraph={{ rows: 8 }} />
      </Card>
    );
  }

  if (!profile) {
    return (
      <Card>
        <Text type="danger">{t('perfil.concessionaria.load.empty')}</Text>
      </Card>
    );
  }

  return (
    <Flex vertical gap="large" style={{ width: '100%' }}>
      <Flex vertical gap="middle">
        <Breadcrumb
          items={[
            { title: <HomeOutlined /> },
            {
              title: (
                <>
                  <UserOutlined /> <span>{t('perfil.breadcrumb')}</span>
                </>
              ),
            },
          ]}
        />

        <Flex align="center" gap="large" wrap="wrap">
          <Avatar size={72} icon={<ShopOutlined />} style={{ background: '#1677ff', flexShrink: 0 }} />
          <Flex vertical gap={4}>
            <Title level={2} style={{ margin: 0 }}>
              {profile.nome}
            </Title>
            <Text type="secondary">{formatCNPJ(profile.cnpj)}</Text>
          </Flex>
        </Flex>
      </Flex>

      <SectionCard
        title={t('perfil.concessionaria.dados.title')}
        icon={<ShopOutlined />}
        editing={editingDados}
        saving={savingDados}
        onEdit={handleEditDados}
        onSave={handleSaveDados}
        onCancel={() => setEditingDados(false)}
      >
        {editingDados ? (
          <Form form={formDados} layout="vertical" style={{ maxWidth: 680 }}>
            <Form.Item
              label={t('perfil.concessionaria.dados.nome.label')}
              name="nome"
              rules={[{ required: true, whitespace: true, message: t('perfil.concessionaria.dados.nome.required') }]}
            >
              <Input prefix={<ShopOutlined />} />
            </Form.Item>

            <Form.Item
              label={t('perfil.dados.email.label')}
              name="email"
              rules={[
                { required: true, message: t('perfil.dados.email.required') },
                { type: 'email', message: t('perfil.dados.email.invalid') },
              ]}
            >
              <Input prefix={<MailOutlined />} />
            </Form.Item>

            <Flex gap="large" wrap="wrap">
              <Form.Item
                label={t('perfil.concessionaria.dados.cnpj.label')}
                name="cnpj"
                style={{ flex: '1 1 220px' }}
              >
                <Input prefix={<IdcardOutlined />} disabled />
              </Form.Item>

              <Form.Item
                label={t('perfil.dados.telefone.label')}
                name="telefone"
                normalize={formatPhone}
                rules={[
                  { required: true, message: t('perfil.dados.telefone.required') },
                  { pattern: PHONE_REGEX, message: t('perfil.dados.telefone.invalid') },
                ]}
                style={{ flex: '1 1 220px' }}
              >
                <Input prefix={<PhoneOutlined />} />
              </Form.Item>
            </Flex>
          </Form>
        ) : (
          <Flex vertical gap="middle">
            <Descriptions column={{ xs: 1, sm: 2 }} size="small">
              <Descriptions.Item label={t('perfil.concessionaria.dados.nome.label')}>
                <Text strong>{profile.nome}</Text>
              </Descriptions.Item>
              <Descriptions.Item label={t('perfil.concessionaria.dados.cnpj.label')}>
                <Flex align="center" gap={6}>
                  <IdcardOutlined style={{ color: '#8c8c8c' }} />
                  <Text>{formatCNPJ(profile.cnpj)}</Text>
                </Flex>
              </Descriptions.Item>
              <Descriptions.Item label={t('perfil.dados.email.label')}>
                <Flex align="center" gap={6}>
                  <MailOutlined style={{ color: '#8c8c8c' }} />
                  <Text>{profile.email || '-'}</Text>
                </Flex>
              </Descriptions.Item>
              <Descriptions.Item label={t('perfil.dados.telefone.label')}>
                <Flex align="center" gap={6}>
                  <PhoneOutlined style={{ color: '#8c8c8c' }} />
                  <Text>{formatPhone(profile.telefone)}</Text>
                </Flex>
              </Descriptions.Item>
            </Descriptions>

            <Divider style={{ margin: '4px 0' }} />

            <Flex align="center" justify="space-between" gap="middle" wrap="wrap">
              <Flex align="center" gap={8}>
                <LockOutlined style={{ color: '#8c8c8c' }} />
                <Text type="secondary">{t('perfil.senha.label')}</Text>
                <Text>********</Text>
              </Flex>
              <Button size="small" icon={<LockOutlined />} onClick={() => setSenhaModalOpen(true)}>
                {t('perfil.senha.change')}
              </Button>
            </Flex>
          </Flex>
        )}
      </SectionCard>

      <SectionCard
        title={t('perfil.concessionaria.endereco.title')}
        icon={<EnvironmentOutlined />}
        editing={false}
      >
        {!profile?.cep ? (
          <Space direction="vertical" size="small" style={{ display: 'flex' }}>
            <Text type="secondary">
              {t('perfil.concessionaria.endereco.empty')}
            </Text>
            <Text type="secondary" style={{ fontSize: 13 }}>
              {t('perfil.concessionaria.endereco.createStorePrefix')}{' '}
              <strong>{t('dashboard.menu.lojas')}</strong>{' '}
              {t('perfil.concessionaria.endereco.createStoreSuffix')}
            </Text>
          </Space>
        ) : (
          <Flex align="flex-start" gap={8}>
            <EnvironmentOutlined style={{ color: '#8c8c8c', marginTop: 3 }} />
            <Flex vertical gap={2}>
              <Text>{enderecoDescription?.linha1 || '-'}</Text>
              <Text type="secondary">{enderecoDescription?.bairro || '-'}</Text>
              <Text type="secondary">{enderecoDescription?.cidadeUf || '-'}</Text>
              <Text type="secondary">
                {t('perfil.endereco.cepDisplay', { cep: formatCEP(enderecoDescription?.cep || '') })}
              </Text>
            </Flex>
          </Flex>
        )}
      </SectionCard>

      <Card
        title={
          <Flex align="center" gap={8}>
            <SettingOutlined />
            <span>{t('perfil.preferencias.title')}</span>
          </Flex>
        }
      >
        <Flex vertical gap="large">
          <Descriptions column={1} size="middle">
            <Descriptions.Item
              label={
                <Flex align="center" gap={8}>
                  <GlobalOutlined />
                  <span>{t('perfil.preferencias.idioma')}</span>
                </Flex>
              }
            >
              <Radio.Group
                value={configuracoes.idioma}
                onChange={(event) => handleIdiomaChange(event.target.value as Idioma)}
              >
                <Space direction="vertical">
                  <Radio value="pt-BR">{t('perfil.preferencias.idioma.ptBr')}</Radio>
                  <Radio value="en-US">{t('perfil.preferencias.idioma.enUs')}</Radio>
                </Space>
              </Radio.Group>
            </Descriptions.Item>

            <Descriptions.Item
              label={
                <Flex align="center" gap={8}>
                  <BulbOutlined />
                  <span>{t('perfil.preferencias.tema')}</span>
                </Flex>
              }
            >
              <Flex align="center" gap={12}>
                <Text>{t('perfil.preferencias.tema.claro')}</Text>
                <Switch checked={configuracoes.tema === 'dark'} onChange={handleTemaChange} />
                <Text>{t('perfil.preferencias.tema.escuro')}</Text>
              </Flex>
            </Descriptions.Item>
          </Descriptions>
          <Text type="secondary">{t('perfil.preferencias.autoSave')}</Text>
        </Flex>
      </Card>

      <Modal
        title={
          <Flex align="center" gap={8}>
            <LockOutlined />
            <span>{t('perfil.senha.change')}</span>
          </Flex>
        }
        open={senhaModalOpen}
        onCancel={() => {
          setSenhaModalOpen(false);
          formSenha.resetFields();
        }}
        onOk={handleAlterarSenha}
        confirmLoading={savingSenha}
        okText={t('perfil.senha.change')}
        cancelText={t('perfil.actions.cancel')}
        destroyOnHidden
      >
        <Form form={formSenha} layout="vertical" style={{ marginTop: 16 }}>
          <Form.Item
            label={t('perfil.senha.current.label')}
            name="senhaAtual"
            rules={[{ required: true, message: t('perfil.senha.current.required') }]}
          >
            <Input.Password />
          </Form.Item>
          <Form.Item
            label={t('perfil.senha.new.label')}
            name="novaSenha"
            rules={[
              { required: true, message: t('perfil.senha.new.required') },
              { min: 6, message: t('perfil.senha.new.min') },
            ]}
          >
            <Input.Password />
          </Form.Item>
          <Form.Item
            label={t('perfil.senha.confirm.label')}
            name="confirmarNovaSenha"
            dependencies={['novaSenha']}
            rules={[
              { required: true, message: t('perfil.senha.confirm.required') },
              ({ getFieldValue }) => ({
                validator(_, value) {
                  if (!value || getFieldValue('novaSenha') === value) {
                    return Promise.resolve();
                  }
                  return Promise.reject(new Error(t('perfil.senha.confirm.match')));
                },
              }),
            ]}
          >
            <Input.Password />
          </Form.Item>
        </Form>
      </Modal>
    </Flex>
  );
}
