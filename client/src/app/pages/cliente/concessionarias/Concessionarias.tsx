import { useEffect, useMemo, useState } from 'react';
import {
  Breadcrumb,
  Typography,
  Input,
  Button,
  Flex,
  Card,
  Divider,
  Empty,
  Tag,
  Select,
  Badge,
  Tooltip,
  Spin,
  theme,
} from 'antd';
import {
  HomeOutlined,
  ShopOutlined,
  SearchOutlined,
  EnvironmentOutlined,
  PhoneOutlined,
  CompassOutlined,
  AimOutlined,
} from '@ant-design/icons';
import { concessionariaService } from '@/app/services/concessionariaService';
import { clienteService } from '@/app/services/clienteService';
import { Loja } from '@/app/models/Loja';
import { handleApiError } from '@/app/utils/errorHandler';
import { formatCEP, formatCNPJ, formatPhone } from '@/app/utils/formatters';
import { t } from '@/app/i18n';
import {
  cidadeUf,
  isLojaPertoCliente,
  ordenarLojasPorProximidade,
} from '@/app/pages/cliente/concessionarias/Concessionarias.utils';

const { Title, Text } = Typography;

interface LojaDisponivel extends Loja {
  key: string;
}

function ConcessionariaCard({
  item,
  perto,
}: {
  item: LojaDisponivel;
  perto: boolean;
}) {
  const { token } = theme.useToken();
  const mapsUrl = `https://www.google.com/maps/dir/?api=1&destination=${encodeURIComponent(
    `${item.logradouro}, ${item.numero}, ${item.bairro}, ${item.cidade}, ${item.uf}, CEP ${item.cep}`
  )}`;
  const isMatriz = item.tipo.toLowerCase() === 'matriz';

  return (
    <Badge.Ribbon
      text={t('clienteConcessionarias.nearRibbon')}
      color="green"
      style={{ display: perto ? 'block' : 'none' }}
    >
      <Card
        hoverable
        style={{ width: '100%' }}
        actions={[
          <Button
            key="ir"
            type="link"
            icon={<CompassOutlined />}
            href={mapsUrl}
            target="_blank"
          >
            {t('lojas.actions.directions')}
          </Button>,
        ]}
      >
        <Flex vertical gap="small">
          <Flex align="flex-start" gap={8} wrap="wrap">
            <Flex vertical gap={4} style={{ flex: 1, minWidth: 0 }}>
              <Text strong style={{ fontSize: 15, width: '100%' }} ellipsis={{ tooltip: item.nome }}>
                {item.nome}
              </Text>
              <Flex align="center" gap={8} wrap="wrap">
                <Tag color={isMatriz ? 'gold' : 'blue'}>
                  {isMatriz ? t('lojas.type.matriz') : t('lojas.type.filial')}
                </Tag>
              </Flex>
              <Text type="secondary" style={{ fontSize: 12 }}>{formatCNPJ(item.cnpj)}</Text>
            </Flex>
          </Flex>

          <Divider style={{ margin: '6px 0' }} />

          <Flex align="flex-start" gap={6}>
            <EnvironmentOutlined style={{ color: token.colorTextTertiary, marginTop: 2, flexShrink: 0 }} />
            <Flex vertical gap={2}>
              <Text style={{ fontSize: 13 }}>{item.logradouro}, {item.numero}</Text>
              <Text type="secondary" style={{ fontSize: 12 }}>{item.bairro} - {cidadeUf(item)}</Text>
              <Text type="secondary" style={{ fontSize: 12 }}>{t('lojas.card.cep', { cep: formatCEP(item.cep) })}</Text>
            </Flex>
          </Flex>

          <Flex align="center" gap={6} style={{ marginTop: 2 }}>
            <PhoneOutlined style={{ color: token.colorTextTertiary, fontSize: 12 }} />
            <Text style={{ fontSize: 13 }}>{formatPhone(item.telefone)}</Text>
          </Flex>
        </Flex>
      </Card>
    </Badge.Ribbon>
  );
}

export default function ConcessionariasCliente() {
  const [lojas, setLojas] = useState<LojaDisponivel[]>([]);
  const [clienteCidade, setClienteCidade] = useState<string | null>(null);
  const [clienteCidadeLabel, setClienteCidadeLabel] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [busca, setBusca] = useState('');
  const [cidadeFiltro, setCidadeFiltro] = useState<string | undefined>(undefined);
  const [apenasProximas, setApenasProximas] = useState(false);

  useEffect(() => {
    const fetchLojas = async () => {
      try {
        setLoading(true);
        const lojasData = await concessionariaService.getLojasAtivas();
        setLojas(lojasData.map((loja) => ({ ...loja, key: loja.id.toString() })));

        try {
          const perfil = await clienteService.getPerfil();
          const cidade = perfil.endereco?.cidade;
          if (cidade) {
            setClienteCidade(cidade);
            setClienteCidadeLabel(cidade);
          }
        } catch {
          setClienteCidade(null);
          setClienteCidadeLabel(null);
        }
      } catch (error) {
        handleApiError(error);
      } finally {
        setLoading(false);
      }
    };

    fetchLojas();
  }, []);

  const cidades = useMemo(() => {
    const set = new Set(lojas.map(cidadeUf));
    return Array.from(set).sort().map((cidade) => ({ value: cidade, label: cidade }));
  }, [lojas]);

  const isProxima = (item: LojaDisponivel) =>
    isLojaPertoCliente(item, clienteCidade);

  const filtradas = useMemo(() => {
    const termo = busca.toLowerCase();
    const digits = busca.replace(/\D/g, '');

    return lojas.filter((loja) => {
      const cidade = cidadeUf(loja);
      const cnpjDigits = loja.cnpj.replace(/\D/g, '');
      const telefoneDigits = loja.telefone.replace(/\D/g, '');
      const matchBusca =
        loja.nome.toLowerCase().includes(termo) ||
        cidade.toLowerCase().includes(termo) ||
        loja.bairro.toLowerCase().includes(termo) ||
        loja.cnpj.includes(busca) ||
        loja.telefone.includes(busca) ||
        (digits.length > 0 && (cnpjDigits.includes(digits) || telefoneDigits.includes(digits)));

      const matchCidade = !cidadeFiltro || cidade === cidadeFiltro;
      const matchProxima = !apenasProximas || isProxima(loja);

      return matchBusca && matchCidade && matchProxima;
    });
  }, [apenasProximas, busca, cidadeFiltro, clienteCidade, lojas]);

  const proximasCount = lojas.filter(isProxima).length;

  return (
    <Spin spinning={loading}>
      <Flex vertical gap="large" style={{ width: '100%' }}>
        <Flex vertical gap="middle">
          <Breadcrumb
            items={[
              { href: '', title: <HomeOutlined /> },
              { title: <><ShopOutlined /><span> {t('clienteConcessionarias.title')}</span></> },
            ]}
          />

          <Title level={2} style={{ margin: 0 }}>
            {t('clienteConcessionarias.title')}
          </Title>

          <Flex gap="middle" wrap="wrap" align="center">
            <Input
              placeholder={t('clienteConcessionarias.search.placeholder')}
              prefix={<SearchOutlined />}
              value={busca}
              onChange={(event) => setBusca(event.target.value)}
              allowClear
              style={{ maxWidth: 340 }}
            />

            <Select
              placeholder={t('clienteConcessionarias.city.placeholder')}
              options={cidades}
              value={cidadeFiltro}
              onChange={setCidadeFiltro}
              allowClear
              style={{ width: 220 }}
            />

            <Tooltip
              title={
                clienteCidade
                  ? t('clienteConcessionarias.near.tooltip.active', { city: clienteCidadeLabel })
                  : t('clienteConcessionarias.near.tooltip.disabled')
              }
            >
              <Button
                type={apenasProximas ? 'primary' : 'default'}
                icon={<AimOutlined />}
                onClick={() => setApenasProximas((value) => !value)}
                disabled={!clienteCidade}
              >
                {t('clienteConcessionarias.near.button')}
                {clienteCidade && proximasCount > 0 && !apenasProximas && (
                  <Tag color="green" style={{ marginLeft: 6, marginRight: -4 }}>
                    {proximasCount}
                  </Tag>
                )}
              </Button>
            </Tooltip>
          </Flex>
        </Flex>

        <Text type="secondary">
          {t('clienteConcessionarias.found', { count: filtradas.length })}
          {apenasProximas && clienteCidade && (
            <Text type="secondary"> {t('clienteConcessionarias.found.near', { city: clienteCidadeLabel })}</Text>
          )}
        </Text>

        {filtradas.length === 0 ? (
          <Card>
            <Empty description={t('clienteConcessionarias.empty')} />
          </Card>
        ) : (
          <Flex wrap="wrap" gap="large">
            {ordenarLojasPorProximidade(filtradas, clienteCidade)
              .map((item) => (
                <div
                  key={item.key}
                  style={{ width: 'calc(33.33% - 16px)', minWidth: 280 }}
                >
                  <ConcessionariaCard item={item} perto={isProxima(item)} />
                </div>
              ))}
          </Flex>
        )}
      </Flex>
    </Spin>
  );
}
