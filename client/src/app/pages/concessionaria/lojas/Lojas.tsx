import { useEffect, useMemo, useState } from 'react';
import {
  Avatar,
  Button,
  Card,
  Divider,
  Empty,
  Flex,
  Image,
  Input,
  message,
  Modal,
  Spin,
  Switch,
  Tag,
  Tooltip,
  Typography,
  theme,
} from 'antd';
import {
  CompassOutlined,
  EditOutlined,
  EnvironmentOutlined,
  IdcardOutlined,
  PlusOutlined,
  SearchOutlined,
  ShopOutlined,
} from '@ant-design/icons';
import DashboardBreadcrumb from '@/app/components/layout/DashboardBreadcrumb';
import { concessionariaService } from '@/app/services/concessionariaService';
import { Loja } from '@/app/models/Loja';
import { handleApiError } from '@/app/utils/errorHandler';
import { getImageUrl } from '@/app/utils/imageUtils';
import { t } from '@/app/i18n';

const { Title, Text } = Typography;

interface LojaData extends Loja {
  key: string;
}

interface LojasProps {
  onNavigateToForm: () => void;
  onNavigateToEdit: (id: number) => void;
}

const isMatriz = (loja: Loja) => loja.tipo.toLowerCase() === 'matriz';

function buildMapsUrl(loja: Loja) {
  return `https://www.google.com/maps/dir/?api=1&destination=${encodeURIComponent(
    `${loja.logradouro}, ${loja.numero}, ${loja.bairro}, ${loja.cidade}, ${loja.uf}, CEP ${loja.cep}`
  )}`;
}

function LojaCover({ loja }: { loja: LojaData }) {
  const { token } = theme.useToken();
  const imageUrl = getImageUrl(loja.foto);

  if (imageUrl) {
    return (
      <Image
        alt={loja.nome}
        src={imageUrl}
        preview={false}
        style={{ height: 160, width: '100%', objectFit: 'cover' }}
      />
    );
  }

  return (
    <Flex
      align="center"
      justify="center"
      style={{
        height: 160,
        background: `linear-gradient(135deg, ${token.colorFillSecondary}, ${token.colorBgLayout})`,
      }}
    >
      <ShopOutlined style={{ color: token.colorTextTertiary, fontSize: 48 }} />
    </Flex>
  );
}

function LojaCard({
  loja,
  onNavigateToEdit,
  onStatusToggle,
}: {
  loja: LojaData;
  onNavigateToEdit: (id: number) => void;
  onStatusToggle: (loja: LojaData) => void;
}) {
  const { token } = theme.useToken();
  const matriz = isMatriz(loja);
  const statusSwitch = (
    <Switch
      checked={loja.ativo}
      disabled={matriz}
      onChange={() => onStatusToggle(loja)}
      checkedChildren={t('status.activeSingle')}
      unCheckedChildren={t('status.inactiveSingle')}
    />
  );

  return (
    <Card
      hoverable
      style={{ width: '100%' }}
      cover={<LojaCover loja={loja} />}
      actions={[
        <Button key="directions" type="link" icon={<CompassOutlined />} href={buildMapsUrl(loja)} target="_blank">
          {t('lojas.actions.directions')}
        </Button>,
        <Button key="edit" type="link" icon={<EditOutlined />} onClick={() => onNavigateToEdit(loja.id)}>
          {t('lojas.actions.edit')}
        </Button>,
        <Flex key="status" justify="center" align="center">
          {matriz ? <Tooltip title={t('lojas.status.matrizLocked')}>{statusSwitch}</Tooltip> : statusSwitch}
        </Flex>,
      ]}
    >
      <Flex vertical gap="small">
        <Flex align="flex-start" gap="middle">
          <Avatar size={40} icon={<ShopOutlined />} src={getImageUrl(loja.foto)} />
          <Flex vertical gap={4} style={{ flex: 1, minWidth: 0 }}>
            <Text strong style={{ fontSize: 15, width: '100%' }} ellipsis={{ tooltip: loja.nome }}>
              {loja.nome}
            </Text>
            <Flex align="center" gap={8} wrap="wrap">
              <Tag color={matriz ? 'gold' : 'blue'}>
                {matriz ? t('lojas.type.matriz') : t('lojas.type.filial')}
              </Tag>
              <Tag color={loja.ativo ? 'green' : 'default'}>
                {loja.ativo ? t('status.activeSingle') : t('status.inactiveSingle')}
              </Tag>
            </Flex>
            <Flex align="center" gap={6}>
              <IdcardOutlined style={{ color: token.colorTextTertiary, fontSize: 12 }} />
              <Text type="secondary" style={{ fontSize: 12 }}>
                {loja.cnpj}
              </Text>
            </Flex>
          </Flex>
        </Flex>

        <Divider style={{ margin: '8px 0' }} />

        <Flex align="flex-start" gap={6}>
          <EnvironmentOutlined style={{ color: token.colorTextTertiary, marginTop: 2 }} />
          <Flex vertical gap={2}>
            <Text style={{ fontSize: 13 }}>{loja.logradouro}, {loja.numero}</Text>
            <Text type="secondary" style={{ fontSize: 12 }}>{loja.bairro} - {loja.cidade}/{loja.uf}</Text>
            <Text type="secondary" style={{ fontSize: 12 }}>{t('lojas.card.cep', { cep: loja.cep })}</Text>
          </Flex>
        </Flex>
      </Flex>
    </Card>
  );
}

export default function Lojas({ onNavigateToForm, onNavigateToEdit }: LojasProps) {
  const [data, setData] = useState<LojaData[]>([]);
  const [loading, setLoading] = useState(true);
  const [busca, setBusca] = useState('');

  const fetchLojas = async () => {
    try {
      setLoading(true);
      const lojas = await concessionariaService.getMinhasLojas();
      setData(lojas.map((loja) => ({ ...loja, key: loja.id.toString() })));
    } catch (error) {
      handleApiError(error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchLojas();
  }, []);

  const lojasFiltradas = useMemo(() => {
    const termo = busca.toLowerCase();
    const digits = busca.replace(/\D/g, '');
    if (!termo) return data;

    return data.filter((loja) => {
      const cnpjDigits = loja.cnpj.replace(/\D/g, '');
      const telefoneDigits = loja.telefone.replace(/\D/g, '');
      return (
        loja.nome.toLowerCase().includes(termo) ||
        loja.cnpj.includes(busca) ||
        (digits.length > 0 && cnpjDigits.includes(digits)) ||
        loja.telefone.includes(busca) ||
        (digits.length > 0 && telefoneDigits.includes(digits)) ||
        loja.logradouro.toLowerCase().includes(termo) ||
        loja.bairro.toLowerCase().includes(termo) ||
        loja.cidade.toLowerCase().includes(termo) ||
        loja.uf.toLowerCase().includes(termo)
      );
    });
  }, [busca, data]);

  const handleStatusToggle = (loja: LojaData) => {
    if (isMatriz(loja)) return;

    const alternar = async () => {
      try {
        setLoading(true);
        await concessionariaService.alternarStatusMinhaLoja(loja.id);
        await fetchLojas();
        message.success(t('lojas.status.success'));
      } catch (error) {
        handleApiError(error, t('lojas.status.error'));
      } finally {
        setLoading(false);
      }
    };

    if (loja.ativo) {
      Modal.confirm({
        title: t('lojas.status.deactivate.title'),
        content: t('lojas.status.deactivate.content', { name: loja.nome }),
        okText: t('yes'),
        cancelText: t('no'),
        onOk: alternar,
      });
      return;
    }

    alternar();
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
              },
            ]}
          />

          <Flex justify="space-between" align="center" wrap="wrap" gap="middle">
            <Title level={2} style={{ margin: 0 }}>
              {t('lojas.title')}
            </Title>
            <Button type="primary" icon={<PlusOutlined />} onClick={onNavigateToForm}>
              {t('lojas.actions.add')}
            </Button>
          </Flex>

          <Input
            placeholder={t('lojas.search.placeholder')}
            prefix={<SearchOutlined />}
            value={busca}
            onChange={(event) => setBusca(event.target.value)}
            allowClear
            style={{ maxWidth: 420 }}
          />
        </Flex>

        <Text type="secondary">{t('lojas.found', { count: lojasFiltradas.length })}</Text>

        {lojasFiltradas.length === 0 ? (
          <Card>
            <Empty description={t('lojas.empty')} />
          </Card>
        ) : (
          <div
            style={{
              display: 'grid',
              gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 320px))',
              gap: 24,
              justifyContent: 'start',
            }}
          >
            {lojasFiltradas.map((loja) => (
              <LojaCard
                key={loja.key}
                loja={loja}
                onNavigateToEdit={onNavigateToEdit}
                onStatusToggle={handleStatusToggle}
              />
            ))}
          </div>
        )}
      </Flex>
    </Spin>
  );
}
