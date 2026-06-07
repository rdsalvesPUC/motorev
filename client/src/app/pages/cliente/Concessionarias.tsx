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

const { Title, Text } = Typography;

interface LojaDisponivel extends Loja {
  key: string;
}

function normalizarCidade(cidade: string): string {
  return cidade.split(' - ')[0].trim().toLowerCase();
}

function cidadeUf(loja: Loja): string {
  return `${loja.cidade} - ${loja.uf}`;
}

function ConcessionariaCard({
  item,
  perto,
}: {
  item: LojaDisponivel;
  perto: boolean;
}) {
  const mapsUrl = `https://www.google.com/maps/dir/?api=1&destination=${encodeURIComponent(
    `${item.logradouro}, ${item.numero}, ${item.bairro}, ${item.cidade}, ${item.uf}, CEP ${item.cep}`
  )}`;
  const isMatriz = item.tipo.toLowerCase() === 'matriz';

  return (
    <Badge.Ribbon
      text="Perto de você"
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
            Ir Para
          </Button>,
        ]}
      >
        <Flex vertical gap="small">
          <Flex align="flex-start" gap={8} wrap="wrap">
            <Flex vertical gap={4} style={{ flex: 1, minWidth: 0 }}>
              <Flex align="center" gap={8} wrap="wrap">
                <Text strong style={{ fontSize: 15 }}>{item.nome}</Text>
                <Tag color={isMatriz ? 'gold' : 'blue'}>
                  {isMatriz ? 'Matriz' : 'Filial'}
                </Tag>
              </Flex>
              <Text type="secondary" style={{ fontSize: 12 }}>{item.cnpj}</Text>
            </Flex>
          </Flex>

          <Divider style={{ margin: '6px 0' }} />

          <Flex align="flex-start" gap={6}>
            <EnvironmentOutlined style={{ color: '#8c8c8c', marginTop: 2, flexShrink: 0 }} />
            <Flex vertical gap={2}>
              <Text style={{ fontSize: 13 }}>{item.logradouro}, {item.numero}</Text>
              <Text type="secondary" style={{ fontSize: 12 }}>{item.bairro} - {cidadeUf(item)}</Text>
              <Text type="secondary" style={{ fontSize: 12 }}>CEP {item.cep}</Text>
            </Flex>
          </Flex>

          <Flex align="center" gap={6} style={{ marginTop: 2 }}>
            <PhoneOutlined style={{ color: '#8c8c8c', fontSize: 12 }} />
            <Text style={{ fontSize: 13 }}>{item.telefone}</Text>
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
            setClienteCidade(normalizarCidade(cidade));
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
    clienteCidade !== null &&
    normalizarCidade(item.cidade) === clienteCidade;

  const filtradas = useMemo(() => {
    const termo = busca.toLowerCase();

    return lojas.filter((loja) => {
      const cidade = cidadeUf(loja);
      const matchBusca =
        loja.nome.toLowerCase().includes(termo) ||
        cidade.toLowerCase().includes(termo) ||
        loja.bairro.toLowerCase().includes(termo) ||
        loja.cnpj.includes(busca);

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
              { title: <><ShopOutlined /><span> Concessionárias</span></> },
            ]}
          />

          <Title level={2} style={{ margin: 0 }}>
            Concessionárias
          </Title>

          <Flex gap="middle" wrap="wrap" align="center">
            <Input
              placeholder="Buscar por nome, cidade, bairro..."
              prefix={<SearchOutlined />}
              value={busca}
              onChange={(event) => setBusca(event.target.value)}
              allowClear
              style={{ maxWidth: 340 }}
            />

            <Select
              placeholder="Filtrar por cidade"
              options={cidades}
              value={cidadeFiltro}
              onChange={setCidadeFiltro}
              allowClear
              style={{ width: 220 }}
            />

            <Tooltip
              title={
                clienteCidade
                  ? `Filtrando por: ${clienteCidadeLabel}`
                  : 'Cadastre seu endereço no Perfil para usar este filtro'
              }
            >
              <Button
                type={apenasProximas ? 'primary' : 'default'}
                icon={<AimOutlined />}
                onClick={() => setApenasProximas((value) => !value)}
                disabled={!clienteCidade}
              >
                Perto de mim
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
          {filtradas.length} concessionária(s) encontrada(s)
          {apenasProximas && clienteCidade && (
            <Text type="secondary"> - próximas a {clienteCidadeLabel}</Text>
          )}
        </Text>

        {filtradas.length === 0 ? (
          <Card>
            <Empty description="Nenhuma concessionária encontrada" />
          </Card>
        ) : (
          <Flex wrap="wrap" gap="large">
            {filtradas
              .slice()
              .sort((a, b) => (isProxima(b) ? 1 : 0) - (isProxima(a) ? 1 : 0))
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
