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
  Tag,
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
import { formatCEP, formatCNPJ, formatPhone } from '@/app/utils/formatters';
import { CEP_REGEX, CNPJ_REGEX, PHONE_REGEX, UF_REGEX, validateCNPJ } from '@/app/utils/validators';

const { Title, Text } = Typography;

interface SectionCardProps {
  title: string;
  icon: React.ReactNode;
  editing: boolean;
  saving?: boolean;
  onEdit: () => void;
  onSave: () => void;
  onCancel: () => void;
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
        editing ? (
          <Flex gap={8} wrap="wrap">
            <Button size="small" icon={<CloseOutlined />} onClick={onCancel} disabled={saving}>
              Cancelar
            </Button>
            <Button size="small" type="primary" icon={<SaveOutlined />} onClick={onSave} loading={saving}>
              Salvar
            </Button>
          </Flex>
        ) : (
          <Button size="small" icon={<EditOutlined />} onClick={onEdit}>
            Editar
          </Button>
        )
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
  const [editingEndereco, setEditingEndereco] = useState(false);
  const [savingDados, setSavingDados] = useState(false);
  const [savingEndereco, setSavingEndereco] = useState(false);
  const [savingSenha, setSavingSenha] = useState(false);
  const [buscandoCep, setBuscandoCep] = useState(false);
  const [enderecoBloqueado, setEnderecoBloqueado] = useState(false);
  const [senhaModalOpen, setSenhaModalOpen] = useState(false);
  const [temaEscuro, setTemaEscuro] = useState(false);
  const [idioma, setIdioma] = useState('pt-BR');

  const [formDados] = Form.useForm<ConcessionariaPerfilRequest>();
  const [formEndereco] = Form.useForm<ConcessionariaPerfilRequest>();
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
          message.error(error.message || 'Falha ao carregar perfil da concessionaria');
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
    tokenManager.updateUserData({ nome: updatedProfile.nome });
  };

  const buildCurrentPayload = (): ConcessionariaPerfilRequest | null => {
    if (!profile) return null;
    return {
      nome: profile.nome,
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
          cnpj: values.cnpj,
          telefone: values.telefone,
        })
      );
      updateProfile(updatedProfile);
      setEditingDados(false);
      message.success('Dados da concessionaria atualizados');
    } catch (error: any) {
      if (error?.errorFields) return;
      message.error(error.message || 'Falha ao atualizar dados da concessionaria');
    } finally {
      setSavingDados(false);
    }
  };

  const handleEditEndereco = () => {
    if (!profile) return;
    formEndereco.setFieldsValue({
      nome: profile.nome,
      cnpj: profile.cnpj,
      telefone: profile.telefone,
      cep: formatCEP(profile.cep),
      logradouro: profile.logradouro,
      numero: profile.numero,
      bairro: profile.bairro,
      cidade: profile.cidade,
      uf: profile.uf,
    });
    setEnderecoBloqueado(false);
    setEditingEndereco(true);
  };

  const handleSaveEndereco = async () => {
    const currentPayload = buildCurrentPayload();
    if (!currentPayload) return;

    try {
      const values = await formEndereco.validateFields();
      setSavingEndereco(true);
      const updatedProfile = await concessionariaService.updateMe(
        buildPerfilPayload({
          ...currentPayload,
          cep: values.cep,
          logradouro: values.logradouro,
          numero: values.numero,
          bairro: values.bairro,
          cidade: values.cidade,
          uf: values.uf,
        })
      );
      updateProfile(updatedProfile);
      setEditingEndereco(false);
      setEnderecoBloqueado(false);
      message.success('Endereco da matriz atualizado');
    } catch (error: any) {
      if (error?.errorFields) return;
      message.error(error.message || 'Falha ao atualizar endereco');
    } finally {
      setSavingEndereco(false);
    }
  };

  const handleBuscarCep = async () => {
    const cep = formEndereco.getFieldValue('cep');
    const digits = normalizeText(cep).replace(/\D/g, '');
    if (digits.length === 0) return;
    if (digits.length !== 8) {
      setEnderecoBloqueado(false);
      message.warning('Informe um CEP com 8 digitos');
      return;
    }

    try {
      setBuscandoCep(true);
      const endereco = await viaCepService.getAddressByCep(cep);
      formEndereco.setFieldsValue({
        cep: formatCEP(endereco.cep),
        logradouro: endereco.logradouro,
        bairro: endereco.bairro,
        cidade: endereco.cidade,
        uf: endereco.uf,
      });
      setEnderecoBloqueado(true);
    } catch (error: any) {
      setEnderecoBloqueado(false);
      message.warning(error.message || 'CEP nao encontrado');
    } finally {
      setBuscandoCep(false);
    }
  };

  const handleCepChange = () => {
    setEnderecoBloqueado(false);
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
      message.success('Senha alterada com sucesso');
    } catch (error: any) {
      if (error?.errorFields) return;
      message.error(error.message || 'Falha ao alterar senha');
    } finally {
      setSavingSenha(false);
    }
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
        <Text type="danger">Nao foi possivel carregar os dados da concessionaria.</Text>
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
                  <UserOutlined /> <span>Perfil</span>
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
            <Flex align="center" gap={8} wrap="wrap">
              <Tag color="gold">{profile.tipo || 'Matriz'}</Tag>
              <Text type="secondary">{formatCNPJ(profile.cnpj)}</Text>
            </Flex>
          </Flex>
        </Flex>
      </Flex>

      <SectionCard
        title="Informacoes da Concessionaria"
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
              label="Nome da Concessionaria"
              name="nome"
              rules={[{ required: true, whitespace: true, message: 'Informe o nome da concessionaria' }]}
            >
              <Input prefix={<ShopOutlined />} />
            </Form.Item>

            <Flex gap="large" wrap="wrap">
              <Form.Item
                label="CNPJ da Matriz"
                name="cnpj"
                normalize={formatCNPJ}
                rules={[
                  { required: true, message: 'Informe o CNPJ' },
                  {
                    validator: (_, value) => {
                      if (!value || (CNPJ_REGEX.test(value) && validateCNPJ(value))) {
                        return Promise.resolve();
                      }
                      return Promise.reject(new Error('CNPJ invalido'));
                    },
                  },
                ]}
                style={{ flex: '1 1 220px' }}
              >
                <Input prefix={<IdcardOutlined />} />
              </Form.Item>

              <Form.Item
                label="Telefone"
                name="telefone"
                normalize={formatPhone}
                rules={[
                  { required: true, message: 'Informe o telefone' },
                  { pattern: PHONE_REGEX, message: 'Telefone invalido' },
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
              <Descriptions.Item label="Nome da Concessionaria">
                <Text strong>{profile.nome}</Text>
              </Descriptions.Item>
              <Descriptions.Item label="Tipo">
                <Tag color="gold">{profile.tipo || 'Matriz'}</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="CNPJ da Matriz">
                <Flex align="center" gap={6}>
                  <IdcardOutlined style={{ color: '#8c8c8c' }} />
                  <Text>{formatCNPJ(profile.cnpj)}</Text>
                </Flex>
              </Descriptions.Item>
              <Descriptions.Item label="Telefone">
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
                <Text type="secondary">Senha</Text>
                <Text>********</Text>
              </Flex>
              <Button size="small" icon={<LockOutlined />} onClick={() => setSenhaModalOpen(true)}>
                Alterar Senha
              </Button>
            </Flex>
          </Flex>
        )}
      </SectionCard>

      <SectionCard
        title="Endereco da Matriz"
        icon={<EnvironmentOutlined />}
        editing={editingEndereco}
        saving={savingEndereco}
        onEdit={handleEditEndereco}
        onSave={handleSaveEndereco}
        onCancel={() => {
          setEditingEndereco(false);
          setEnderecoBloqueado(false);
        }}
      >
        {editingEndereco ? (
          <Form form={formEndereco} layout="vertical" style={{ maxWidth: 760 }}>
            <Flex gap="large" wrap="wrap">
              <Form.Item
                label="CEP"
                name="cep"
                normalize={formatCEP}
                rules={[
                  { required: true, message: 'Informe o CEP' },
                  { pattern: CEP_REGEX, message: 'CEP invalido' },
                ]}
                style={{ flex: '0 0 170px' }}
              >
                <Input placeholder="00000-000" onChange={handleCepChange} onBlur={handleBuscarCep} />
              </Form.Item>

              <Form.Item
                label="Rua / Avenida"
                name="logradouro"
                rules={[{ required: true, whitespace: true, message: 'Informe o logradouro' }]}
                style={{ flex: '1 1 280px' }}
              >
                <Input placeholder="Ex: Av. Paulista" disabled={buscandoCep || enderecoBloqueado} />
              </Form.Item>

              <Form.Item
                label="Numero"
                name="numero"
                rules={[{ required: true, whitespace: true, message: 'Informe o numero' }]}
                style={{ flex: '0 0 120px' }}
              >
                <Input placeholder="Ex: 1000" />
              </Form.Item>
            </Flex>

            <Flex gap="large" wrap="wrap">
              <Form.Item
                label="Bairro"
                name="bairro"
                rules={[{ required: true, whitespace: true, message: 'Informe o bairro' }]}
                style={{ flex: '1 1 220px' }}
              >
                <Input disabled={buscandoCep || enderecoBloqueado} />
              </Form.Item>

              <Form.Item
                label="Cidade"
                name="cidade"
                rules={[{ required: true, whitespace: true, message: 'Informe a cidade' }]}
                style={{ flex: '1 1 220px' }}
              >
                <Input disabled={buscandoCep || enderecoBloqueado} />
              </Form.Item>

              <Form.Item
                label="UF"
                name="uf"
                rules={[
                  { required: true, message: 'Informe a UF' },
                  { pattern: UF_REGEX, message: 'UF deve conter 2 letras' },
                ]}
                style={{ flex: '0 0 100px' }}
              >
                <Input
                  placeholder="SP"
                  maxLength={2}
                  disabled={buscandoCep || enderecoBloqueado}
                  onChange={(event) => formEndereco.setFieldValue('uf', event.target.value.toUpperCase())}
                />
              </Form.Item>
            </Flex>
          </Form>
        ) : (
          <Flex align="flex-start" gap={8}>
            <EnvironmentOutlined style={{ color: '#8c8c8c', marginTop: 3 }} />
            <Flex vertical gap={2}>
              <Text>{enderecoDescription?.linha1 || '-'}</Text>
              <Text type="secondary">{enderecoDescription?.bairro || '-'}</Text>
              <Text type="secondary">{enderecoDescription?.cidadeUf || '-'}</Text>
              <Text type="secondary">CEP {formatCEP(enderecoDescription?.cep || '')}</Text>
            </Flex>
          </Flex>
        )}
      </SectionCard>

      <Card
        title={
          <Flex align="center" gap={8}>
            <SettingOutlined />
            <span>Preferencias</span>
          </Flex>
        }
      >
        <Flex vertical gap="large">
          <Descriptions column={1} size="middle">
            <Descriptions.Item
              label={
                <Flex align="center" gap={8}>
                  <GlobalOutlined />
                  <span>Idioma</span>
                </Flex>
              }
            >
              <Radio.Group value={idioma} onChange={(event) => setIdioma(event.target.value)}>
                <Space direction="vertical">
                  <Radio value="pt-BR">Portugues (Brasil)</Radio>
                  <Radio value="en-US">English (United States)</Radio>
                </Space>
              </Radio.Group>
            </Descriptions.Item>

            <Descriptions.Item
              label={
                <Flex align="center" gap={8}>
                  <BulbOutlined />
                  <span>Tema</span>
                </Flex>
              }
            >
              <Flex align="center" gap={12}>
                <Text>Claro</Text>
                <Switch checked={temaEscuro} onChange={setTemaEscuro} />
                <Text>Escuro</Text>
              </Flex>
            </Descriptions.Item>
          </Descriptions>
          <Text type="secondary">Preferencias mantidas apenas no Front por enquanto.</Text>
        </Flex>
      </Card>

      <Modal
        title={
          <Flex align="center" gap={8}>
            <LockOutlined />
            <span>Alterar Senha</span>
          </Flex>
        }
        open={senhaModalOpen}
        onCancel={() => {
          setSenhaModalOpen(false);
          formSenha.resetFields();
        }}
        onOk={handleAlterarSenha}
        confirmLoading={savingSenha}
        okText="Alterar Senha"
        cancelText="Cancelar"
        destroyOnHidden
      >
        <Form form={formSenha} layout="vertical" style={{ marginTop: 16 }}>
          <Form.Item
            label="Senha Atual"
            name="senhaAtual"
            rules={[{ required: true, message: 'Informe a senha atual' }]}
          >
            <Input.Password />
          </Form.Item>
          <Form.Item
            label="Nova Senha"
            name="novaSenha"
            rules={[
              { required: true, message: 'Informe a nova senha' },
              { min: 6, message: 'A senha deve ter no minimo 6 caracteres' },
            ]}
          >
            <Input.Password />
          </Form.Item>
          <Form.Item
            label="Confirmar Nova Senha"
            name="confirmarNovaSenha"
            dependencies={['novaSenha']}
            rules={[
              { required: true, message: 'Confirme a nova senha' },
              ({ getFieldValue }) => ({
                validator(_, value) {
                  if (!value || getFieldValue('novaSenha') === value) {
                    return Promise.resolve();
                  }
                  return Promise.reject(new Error('As senhas nao coincidem'));
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
