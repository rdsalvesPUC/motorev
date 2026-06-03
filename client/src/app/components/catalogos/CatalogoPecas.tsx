import { useEffect, useMemo, useState } from 'react';
import { Breadcrumb, Typography, Input, Select, Button, Table, Space, Flex, Tag, Card, Alert, message } from 'antd';
import { HomeOutlined, ToolOutlined, SearchOutlined, ReloadOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { useNavigate } from 'react-router';
import { CATEGORIAS_PECA, pecaService, type PecaResponse } from '../../services/pecaService';
import { PATHS } from '../../paths';

const { Title, Text } = Typography;

type EstoqueFilter = 'disponivel' | 'baixo' | 'zerado';

function formatCurrency(value: number) {
  return value.toLocaleString('pt-BR', {
    style: 'currency',
    currency: 'BRL',
  });
}

function getEstoqueTag(estoque: number) {
  const color = estoque > 10 ? 'green' : estoque > 0 ? 'orange' : 'red';
  return <Tag color={color}>{estoque} unidades</Tag>;
}

export default function CatalogoPecas() {
  const navigate = useNavigate();
  const [pecas, setPecas] = useState<PecaResponse[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [categoria, setCategoria] = useState<string>();
  const [estoque, setEstoque] = useState<EstoqueFilter>();

  const loadPecas = async () => {
    setLoading(true);
    setError('');

    try {
      const data = await pecaService.listar();
      setPecas(data);
    } catch (err: any) {
      const errorMessage = err.message || 'Não foi possível carregar o catálogo de peças.';
      setError(errorMessage);
      message.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadPecas();
  }, []);

  const filteredPecas = useMemo(() => {
    const normalizedSearch = search.trim().toLowerCase();

    return pecas.filter((peca) => {
      const matchesSearch =
        !normalizedSearch ||
        peca.codigo.toLowerCase().includes(normalizedSearch) ||
        peca.nome.toLowerCase().includes(normalizedSearch);

      const matchesCategoria = !categoria || peca.categoria === categoria;

      const matchesEstoque =
        !estoque ||
        (estoque === 'disponivel' && peca.estoque > 10) ||
        (estoque === 'baixo' && peca.estoque > 0 && peca.estoque <= 10) ||
        (estoque === 'zerado' && peca.estoque === 0);

      return matchesSearch && matchesCategoria && matchesEstoque;
    });
  }, [pecas, search, categoria, estoque]);

  const clearFilters = () => {
    setSearch('');
    setCategoria(undefined);
    setEstoque(undefined);
  };

  const columns: ColumnsType<PecaResponse> = [
    {
      title: 'Código',
      dataIndex: 'codigo',
      key: 'codigo',
      sorter: (a, b) => a.codigo.localeCompare(b.codigo),
    },
    {
      title: 'Nome da Peça',
      dataIndex: 'nome',
      key: 'nome',
      sorter: (a, b) => a.nome.localeCompare(b.nome),
    },
    {
      title: 'Categoria',
      dataIndex: 'categoria',
      key: 'categoria',
      filters: CATEGORIAS_PECA.map((item) => ({ text: item.label, value: item.value })),
      onFilter: (value, record) => record.categoria === value,
    },
    {
      title: 'Preço',
      dataIndex: 'preco',
      key: 'preco',
      align: 'right',
      render: (preco: number) => formatCurrency(preco),
      sorter: (a, b) => a.preco - b.preco,
    },
    {
      title: 'Estoque',
      dataIndex: 'estoque',
      key: 'estoque',
      render: getEstoqueTag,
      sorter: (a, b) => a.estoque - b.estoque,
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => <Tag color={status === 'Ativo' ? 'blue' : 'default'}>{status}</Tag>,
    },
  ];

  return (
    <Space direction="vertical" size="large" style={{ width: '100%' }}>
      <Space direction="vertical" size="middle" style={{ width: '100%' }}>
        <Breadcrumb
          items={[
            {
              title: <HomeOutlined />,
            },
            {
              title: (
                <>
                  <ToolOutlined />
                  <span>Catálogos</span>
                </>
              ),
            },
            {
              title: 'Peças',
            },
          ]}
        />

        <Title level={2} style={{ margin: 0 }}>
          Peças
        </Title>

        <Flex gap="middle" align="center" wrap="wrap">
          <Input
            placeholder="Buscar peças..."
            prefix={<SearchOutlined />}
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            style={{ width: 300 }}
            allowClear
          />

          <Select
            placeholder="Categoria"
            value={categoria}
            onChange={setCategoria}
            style={{ width: 160 }}
            options={CATEGORIAS_PECA}
            allowClear
          />

          <Select
            placeholder="Estoque"
            value={estoque}
            onChange={setEstoque}
            style={{ width: 160 }}
            options={[
              { value: 'disponivel', label: 'Disponível' },
              { value: 'baixo', label: 'Estoque Baixo' },
              { value: 'zerado', label: 'Sem Estoque' },
            ]}
            allowClear
          />

          <Flex gap="small" style={{ marginLeft: 'auto' }}>
            <Button onClick={clearFilters}>Limpar</Button>
            <Button icon={<ReloadOutlined />} onClick={loadPecas} loading={loading}>
              Atualizar
            </Button>
          </Flex>
        </Flex>
      </Space>

      {error && <Alert type="error" message={error} showIcon />}

      <Card>
        <Space direction="vertical" size="middle" style={{ width: '100%' }}>
          <Flex justify="space-between" align="center" wrap="wrap" gap="middle">
            <Text>Total: {filteredPecas.length} peças</Text>
            <Button
              type="primary"
              onClick={() => navigate(PATHS.CONCESSIONARIA_CATALOGOS_PECAS_CREATE)}
            >
              Adicionar Peça
            </Button>
          </Flex>

          <Table
            bordered
            loading={loading}
            dataSource={filteredPecas}
            columns={columns}
            rowKey="id"
            pagination={{ pageSize: 10, showSizeChanger: true }}
          />
        </Space>
      </Card>
    </Space>
  );
}
