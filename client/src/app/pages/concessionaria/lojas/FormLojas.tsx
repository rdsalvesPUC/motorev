import { useEffect, useState } from 'react';
import { useParams } from 'react-router';
import { Alert, Button, Card, Flex, Form, Input, message, Spin, Switch, Typography, Upload } from 'antd';
import { ArrowLeftOutlined, EnvironmentOutlined, IdcardOutlined, PlusOutlined, ShopOutlined } from '@ant-design/icons';
import type { UploadFile } from 'antd/es/upload/interface';
import DashboardBreadcrumb from '@/app/components/layout/DashboardBreadcrumb';
import { concessionariaService } from '@/app/services/concessionariaService';
import { viaCepService } from '@/app/services/viaCepService';
import { LojaRequest } from '@/app/models/LojaRequest';
import { handleApiError } from '@/app/utils/errorHandler';
import { formatCEP, formatCNPJ, formatPhone } from '@/app/utils/formatters';
import { CEP_REGEX, CNPJ_REGEX, PHONE_REGEX, UF_REGEX, validateCNPJ } from '@/app/utils/validators';
import { getImageUrl } from '@/app/utils/imageUtils';
import { PATH_SEGMENTS } from '@/app/paths';
import { t } from '@/app/i18n';

const { Title, Text } = Typography;

interface LojasCreateProps {
  onBack: () => void;
}

const isMatrizTipo = (tipo?: string) => tipo?.toLowerCase() === 'matriz';

function normalizeText(value?: string | null) {
  return value?.trim() || '';
}

export default function FormLojas({ onBack }: LojasCreateProps) {
  const [form] = Form.useForm<LojaRequest>();
  const { id } = useParams();
  const lojaId = id ? Number(id) : undefined;
  const isEditing = lojaId !== undefined && !Number.isNaN(lojaId);
  const [loading, setLoading] = useState(false);
  const [buscandoCep, setBuscandoCep] = useState(false);
  const [enderecoBloqueado, setEnderecoBloqueado] = useState(false);
  const [hasMatriz, setHasMatriz] = useState(true);
  const [lojaTipo, setLojaTipo] = useState('Filial');
  const [fotoUrl, setFotoUrl] = useState<string | undefined>(undefined);
  const [fileList, setFileList] = useState<UploadFile[]>([]);

  const isEditingMatriz = isEditing && isMatrizTipo(lojaTipo);
  const matrizLocked = isEditingMatriz || (!isEditing && !hasMatriz);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const minhasLojas = await concessionariaService.getMinhasLojas();
        const existsMatriz = minhasLojas.some((loja) => isMatrizTipo(loja.tipo));
        setHasMatriz(existsMatriz);

        if (isEditing && lojaId) {
          const loja = await concessionariaService.getMinhaLojaById(lojaId);
          setLojaTipo(loja.tipo);
          setFotoUrl(loja.foto);
          form.setFieldsValue({
            nome: loja.nome,
            cnpj: loja.cnpj,
            telefone: loja.telefone,
            cep: loja.cep,
            logradouro: loja.logradouro,
            numero: loja.numero,
            bairro: loja.bairro,
            cidade: loja.cidade,
            uf: loja.uf,
            isMatriz: isMatrizTipo(loja.tipo),
            foto: loja.foto,
          });

          if (loja.foto) {
            setFileList([
              {
                uid: '-1',
                name: 'loja.png',
                status: 'done',
                url: getImageUrl(loja.foto),
              },
            ]);
          }
        } else {
          form.setFieldsValue({ isMatriz: !existsMatriz });
        }
      } catch (error) {
        handleApiError(error);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [form, isEditing, lojaId]);

  const customUpload = async (options: any) => {
    const { onSuccess, onError, file } = options;
    try {
      const res = await concessionariaService.uploadImage(file as File);
      setFotoUrl(res.url);
      file.status = 'done';
      file.url = getImageUrl(res.url);
      onSuccess(res, file);
      message.success(t('lojas.form.foto.success'));
    } catch (error) {
      file.status = 'error';
      onError(error);
      handleApiError(error, t('lojas.form.foto.error'));
    }
  };

  const handleUploadChange = ({ fileList: newFileList }: { fileList: UploadFile[] }) => {
    if (newFileList.length === 0) {
      setFotoUrl(undefined);
    }
    setFileList(newFileList);
  };

  const buildPayload = (values: LojaRequest): LojaRequest => ({
    nome: normalizeText(values.nome),
    cnpj: normalizeText(values.cnpj),
    telefone: normalizeText(values.telefone),
    cep: normalizeText(values.cep),
    logradouro: normalizeText(values.logradouro),
    numero: normalizeText(values.numero),
    bairro: normalizeText(values.bairro),
    cidade: normalizeText(values.cidade),
    uf: normalizeText(values.uf).toUpperCase(),
    isMatriz: matrizLocked ? true : Boolean(values.isMatriz),
    foto: fotoUrl,
  });

  const handleSubmit = async (values: LojaRequest) => {
    try {
      setLoading(true);
      const payload = buildPayload(values);
      if (isEditing && lojaId) {
        await concessionariaService.updateMinhaLoja(lojaId, payload);
        message.success(t('lojas.form.update.success'));
      } else {
        await concessionariaService.createMinhaLoja(payload);
        message.success(t('lojas.form.create.success'));
      }
      form.resetFields();
      onBack();
    } catch (error) {
      handleApiError(error, isEditing ? t('lojas.form.update.error') : t('lojas.form.create.error'));
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = () => {
    form.resetFields();
    setEnderecoBloqueado(false);
    onBack();
  };

  const handleCepChange = () => {
    setEnderecoBloqueado(false);
  };

  const handleBuscarCep = async () => {
    const cep = form.getFieldValue('cep');
    const digits = String(cep || '').replace(/\D/g, '');
    if (digits.length === 0) return;
    if (digits.length !== 8) {
      setEnderecoBloqueado(false);
      message.warning(t('lojas.form.cep.invalidLength'));
      return;
    }

    try {
      setBuscandoCep(true);
      const endereco = await viaCepService.buscarEnderecoPorCep(cep);
      if (!endereco) {
        throw new Error(t('lojas.form.cep.notFound'));
      }
      form.setFieldsValue({
        cep: formatCEP(endereco.cep),
        logradouro: endereco.logradouro,
        bairro: endereco.bairro,
        cidade: endereco.cidade,
        uf: endereco.uf,
      });
      setEnderecoBloqueado(true);
    } catch (error: any) {
      setEnderecoBloqueado(false);
      message.warning(error.message || t('lojas.form.cep.notFound'));
    } finally {
      setBuscandoCep(false);
    }
  };

  return (
    <Spin spinning={loading}>
      <Flex vertical gap="large" style={{ width: '100%' }}>
        <Flex vertical gap="middle">
          <DashboardBreadcrumb
            userType="concessionaria"
            items={[
              {
                title: t('lojas.title'),
                icon: <ShopOutlined />,
                path: PATH_SEGMENTS.CONCESSIONARIA_LOJAS,
              },
              {
                title: isEditing ? t('lojas.form.title.edit') : t('lojas.form.title.create'),
              },
            ]}
          />

          <Flex align="center" gap="middle">
            <Button icon={<ArrowLeftOutlined />} onClick={handleCancel} />
            <Title level={2} style={{ margin: 0 }}>
              {isEditing ? t('lojas.form.title.edit') : t('lojas.form.title.create')}
            </Title>
          </Flex>
        </Flex>

        <Form form={form} layout="vertical" onFinish={handleSubmit} style={{ width: '100%', maxWidth: 920 }}>
          <Card
            title={
              <Flex align="center" gap={8}>
                <ShopOutlined />
                <span>{t('lojas.form.section.store')}</span>
              </Flex>
            }
            style={{ marginBottom: 24 }}
          >
            {matrizLocked && (
              <Alert
                type="info"
                showIcon
                message={isEditingMatriz ? t('lojas.form.matriz.editLocked') : t('lojas.form.matriz.firstStore')}
                style={{ marginBottom: 16 }}
              />
            )}

            <Flex gap="large" wrap="wrap">
              <Form.Item
                label={t('lojas.form.nome.label')}
                name="nome"
                rules={[{ required: true, whitespace: true, message: t('lojas.form.nome.required') }]}
                style={{ flex: '1 1 360px' }}
              >
                <Input placeholder={t('lojas.form.nome.placeholder')} />
              </Form.Item>

              <Form.Item
                label={t('lojas.form.matriz.label')}
                name="isMatriz"
                valuePropName="checked"
                style={{ flex: '0 0 180px' }}
              >
                <Switch
                  disabled={matrizLocked}
                  checkedChildren={t('lojas.type.matriz')}
                  unCheckedChildren={t('lojas.type.filial')}
                />
              </Form.Item>
            </Flex>

            <Flex gap="large" wrap="wrap">
              <Form.Item
                label={t('lojas.form.cnpj.label')}
                name="cnpj"
                normalize={formatCNPJ}
                rules={[
                  { required: true, message: t('lojas.form.cnpj.required') },
                  {
                    validator: (_, value) => {
                      if (!value || (CNPJ_REGEX.test(value) && validateCNPJ(value))) {
                        return Promise.resolve();
                      }
                      return Promise.reject(new Error(t('lojas.form.cnpj.invalid')));
                    },
                  },
                ]}
                style={{ flex: '1 1 280px' }}
              >
                <Input prefix={<IdcardOutlined />} placeholder={t('lojas.form.cnpj.placeholder')} />
              </Form.Item>

              <Form.Item
                label={t('lojas.form.telefone.label')}
                name="telefone"
                normalize={formatPhone}
                rules={[
                  { required: true, message: t('lojas.form.telefone.required') },
                  { pattern: PHONE_REGEX, message: t('lojas.form.telefone.invalid') },
                ]}
                style={{ flex: '1 1 260px' }}
              >
                <Input placeholder={t('lojas.form.telefone.placeholder')} />
              </Form.Item>
            </Flex>
          </Card>

          <Card
            title={
              <Flex align="center" gap={8}>
                <EnvironmentOutlined />
                <span>{t('lojas.form.section.address')}</span>
              </Flex>
            }
            style={{ marginBottom: 24 }}
          >
            <Flex gap="large" wrap="wrap">
              <Form.Item
                label={t('lojas.form.cep.label')}
                name="cep"
                normalize={formatCEP}
                rules={[
                  { required: true, message: t('lojas.form.cep.required') },
                  { pattern: CEP_REGEX, message: t('lojas.form.cep.invalid') },
                ]}
                style={{ flex: '0 0 160px' }}
              >
                <Input placeholder={t('lojas.form.cep.placeholder')} onChange={handleCepChange} onBlur={handleBuscarCep} />
              </Form.Item>

              <Form.Item
                label={t('lojas.form.logradouro.label')}
                name="logradouro"
                rules={[{ required: true, whitespace: true, message: t('lojas.form.logradouro.required') }]}
                style={{ flex: '1 1 360px' }}
              >
                <Input placeholder={t('lojas.form.logradouro.placeholder')} disabled={buscandoCep || enderecoBloqueado} />
              </Form.Item>

              <Form.Item
                label={t('lojas.form.numero.label')}
                name="numero"
                rules={[{ required: true, whitespace: true, message: t('lojas.form.numero.required') }]}
                style={{ flex: '0 0 140px' }}
              >
                <Input placeholder={t('lojas.form.numero.placeholder')} />
              </Form.Item>
            </Flex>

            <Flex gap="large" wrap="wrap">
              <Form.Item
                label={t('lojas.form.bairro.label')}
                name="bairro"
                rules={[{ required: true, whitespace: true, message: t('lojas.form.bairro.required') }]}
                style={{ flex: '1 1 260px' }}
              >
                <Input placeholder={t('lojas.form.bairro.placeholder')} disabled={buscandoCep || enderecoBloqueado} />
              </Form.Item>

              <Form.Item
                label={t('lojas.form.cidade.label')}
                name="cidade"
                rules={[{ required: true, whitespace: true, message: t('lojas.form.cidade.required') }]}
                style={{ flex: '1 1 260px' }}
              >
                <Input placeholder={t('lojas.form.cidade.placeholder')} disabled={buscandoCep || enderecoBloqueado} />
              </Form.Item>

              <Form.Item
                label={t('lojas.form.uf.label')}
                name="uf"
                rules={[
                  { required: true, message: t('lojas.form.uf.required') },
                  { pattern: UF_REGEX, message: t('lojas.form.uf.invalid') },
                ]}
                style={{ flex: '0 0 120px' }}
              >
                <Input
                  placeholder={t('lojas.form.uf.placeholder')}
                  maxLength={2}
                  disabled={buscandoCep || enderecoBloqueado}
                  onChange={(event) => form.setFieldValue('uf', event.target.value.toUpperCase())}
                />
              </Form.Item>
            </Flex>
          </Card>

          <Card title={t('lojas.form.section.photo')} style={{ marginBottom: 24 }}>
            <Form.Item label={t('lojas.form.foto.label')} extra={t('lojas.form.foto.hint')}>
              <Upload
                listType="picture-card"
                fileList={fileList}
                customRequest={customUpload}
                onChange={handleUploadChange}
                maxCount={1}
                beforeUpload={(file) => {
                  const isImage = file.type.startsWith('image/');
                  if (!isImage) {
                    message.error(t('lojas.form.foto.invalidType'));
                    return Upload.LIST_IGNORE;
                  }
                  return true;
                }}
              >
                {fileList.length < 1 && (
                  <div>
                    <PlusOutlined />
                    <div style={{ marginTop: 8 }}>{t('lojas.form.foto.upload')}</div>
                  </div>
                )}
              </Upload>
            </Form.Item>
            <Text type="secondary">{t('lojas.form.foto.description')}</Text>
          </Card>

          <Flex gap="middle" wrap="wrap">
            <Button onClick={handleCancel}>{t('lojas.form.cancel')}</Button>
            <Button type="primary" htmlType="submit" loading={loading}>
              {isEditing ? t('lojas.form.submit.edit') : t('lojas.form.submit.create')}
            </Button>
          </Flex>
        </Form>
      </Flex>
    </Spin>
  );
}
