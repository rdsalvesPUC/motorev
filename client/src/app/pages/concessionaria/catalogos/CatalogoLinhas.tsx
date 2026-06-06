import React, { useState, useEffect, useMemo } from 'react';
import { Breadcrumb, Typography, Input, Button, Table, Space, Flex, Form, Switch, Tag, Spin, message, Popconfirm, Select } from 'antd';
import { HomeOutlined, ToolOutlined, SearchOutlined, EditOutlined } from '@ant-design/icons';
import type { ColumnType } from 'antd/es/table';
import { linhaService } from '@/app/services/linhaService';
import { modeloMotoService } from '@/app/services/modeloMotoService';
import { Linha } from '@/app/models/Linha';
import { ModeloMoto } from '@/app/models/ModeloMoto';
import { handleApiError } from '@/app/utils/errorHandler';
import { t } from '@/app/i18n';

const { Title } = Typography;

interface LinhaData extends Linha {
    key: string;
}

interface CatalogoLinhasProps {
    onNavigateToForm?: () => void;
}

interface EditableColumn extends ColumnType<LinhaData> {
    editable?: boolean;
}

interface EditableCellProps {
    editing: boolean;
    dataIndex: string;
    title: string;
    children: React.ReactNode;
}

const EditableCell: React.FC<EditableCellProps> = ({
                                                       editing,
                                                       dataIndex,
                                                       children,
                                                       ...restProps
                                                   }) => {
    const inputNode = <Input />;

    return (
        <td {...restProps}>
            {editing ? (
                <Form.Item
                    name={dataIndex}
                    style={{ margin: 0 }}
                    rules={[
                        {
                            required: true,
                            message: `Campo obrigatório!`,
                        },
                    ]}
                >
                    {inputNode}
                </Form.Item>
            ) : (
                children
            )}
        </td>
    );
};

export default function CatalogoLinhas({ onNavigateToForm }: CatalogoLinhasProps) {
    const [form] = Form.useForm();
    const [data, setData] = useState<LinhaData[]>([]);
    const [modelos, setModelos] = useState<ModeloMoto[]>([]);
    const [loading, setLoading] = useState(true);
    const [editingKey, setEditingKey] = useState('');
    const [searchText, setSearchText] = useState('');
    const [statusFilter, setStatusFilter] = useState<'Ativo' | 'Inativo' | 'Todos'>('Ativo');

    const fetchLinhas = async () => {
        try {
            setLoading(true);
            const apenasAtivos = statusFilter === 'Ativo';
            const linhas = await linhaService.getAll(apenasAtivos);
            const models = await modeloMotoService.getAll();
            
            const filteredLinhas = statusFilter === 'Inativo'
                ? linhas.filter(l => !l.ativo)
                : linhas;

            setData(filteredLinhas.map(l => ({ ...l, key: l.id.toString() })));
            setModelos(models);
        } catch (error) {
            handleApiError(error, 'error.fetchLinhas');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchLinhas();
    }, [statusFilter]);

    const filteredData = useMemo(() => {
        if (!searchText) return data;
        const lowerSearch = searchText.toLowerCase();
        return data.filter(item =>
            item.nome.toLowerCase().includes(lowerSearch) ||
            (item.descricao && item.descricao.toLowerCase().includes(lowerSearch))
        );
    }, [data, searchText]);

    const isEditing = (record: LinhaData) => record.key === editingKey;

    const edit = (record: LinhaData) => {
        form.setFieldsValue({ ...record });
        setEditingKey(record.key);
    };

    const cancel = () => {
        setEditingKey('');
    };

    const save = async (key: string) => {
        try {
            const row = await form.validateFields();
            const record = data.find(item => item.key === key);
            if (!record) return;

            setLoading(true);
            await linhaService.update(record.id, {
                nome: row.nome,
                descricao: row.descricao
            });
            setEditingKey('');
            await fetchLinhas();
            message.success(t('linhaUpdatedSuccess'));
        } catch (error) {
            handleApiError(error);
        } finally {
            setLoading(false);
        }
    };

    const handleStatusToggle = async (record: LinhaData) => {
        if (record.ativo) {
            const temModelos = modelos.some((m) => m.linhaId === record.id && m.ativo);
            if (temModelos) {
                message.error("Não é possível desativar uma linha com modelos de motos vinculados.");
                return;
            }
        }

        try {
            setLoading(true);
            await linhaService.alternarStatus(record.id);
            await fetchLinhas();
            message.success(t('linhaStatusUpdatedSuccess'));
        } catch (error) {
            handleApiError(error);
        } finally {
            setLoading(false);
        }
    };

    const columns: EditableColumn[] = [
        {
            title: 'Nome da Linha',
            dataIndex: 'nome',
            key: 'nome',
            editable: true,
        },
        {
            title: 'Descrição',
            dataIndex: 'descricao',
            key: 'descricao',
            editable: true,
        },
        {
            title: 'Motos Vinculadas',
            key: 'motosVinculadas',
            width: 160,
            align: 'center',
            render: (_: any, record: LinhaData) =>
                modelos.filter((m) => m.linhaId === record.id).length,
        },
        {
            title: 'Status',
            key: 'status',
            width: 120,
            align: 'center',
            render: (_: any, record: LinhaData) => {
                const editable = isEditing(record);
                return editable ? (
                    record.ativo ? <Tag color="green">Ativo</Tag> : <Tag color="red">Inativo</Tag>
                ) : (
                    <Popconfirm
                        title="Desativar linha"
                        description="Tem certeza que deseja desativar esta linha?"
                        onConfirm={() => handleStatusToggle(record)}
                        okText="Sim"
                        cancelText="Não"
                        disabled={!record.ativo}
                    >
                        <Switch
                            checked={record.ativo}
                            onChange={(checked) => {
                                if (checked) {
                                    handleStatusToggle(record);
                                }
                            }}
                            checkedChildren="Ativo"
                            unCheckedChildren="Inativo"
                        />
                    </Popconfirm>
                );
            },
        },
        {
            title: 'Ações',
            key: 'actions',
            width: 1,
            render: (_: any, record: LinhaData) => {
                const editable = isEditing(record);
                return editable ? (
                    <Space>
                        <Popconfirm
                            title="Salvar alterações"
                            description="Tem certeza que deseja salvar as alterações?"
                            onConfirm={() => save(record.key)}
                            okText="Sim"
                            cancelText="Não"
                        >
                            <Button key="save" type="link">
                                Salvar
                            </Button>
                        </Popconfirm>
                        <Popconfirm
                            title="Cancelar edição"
                            description="Tem certeza que deseja cancelar as alterações?"
                            onConfirm={cancel}
                            okText="Sim"
                            cancelText="Não"
                        >
                            <Button key="cancel" type="link" danger>
                                Cancelar
                            </Button>
                        </Popconfirm>
                    </Space>
                ) : (
                    <Button
                        key="edit"
                        type="link"
                        icon={<EditOutlined />}
                        disabled={editingKey !== ''}
                        onClick={() => edit(record)}
                    >
                        Editar
                    </Button>
                );
            },
        },
    ];

    const mergedColumns = columns.map((col) => {
        if (!col.editable) {
            return col;
        }
        return {
            ...col,
            onCell: (record: LinhaData) => ({
                record,
                dataIndex: col.dataIndex,
                title: col.title as string,
                editing: isEditing(record),
            }),
        };
    });

    return (
        <Spin spinning={loading}>
            <Flex vertical gap="large" style={{ width: '100%' }}>
                <Flex vertical gap="middle" style={{ width: '100%' }}>
                    <Breadcrumb
                        items={[
                            {
                                href: '',
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
                                title: 'Linhas de Motos',
                            },
                        ]}
                    />

                    <Title level={2} style={{ margin: 0 }}>
                        Linhas de Motos
                    </Title>

                    <Flex gap="middle" align="center" wrap="wrap">
                        <Input
                            placeholder="Buscar linhas..."
                            prefix={<SearchOutlined />}
                            style={{ width: 300 }}
                            value={searchText}
                            onChange={(e) => setSearchText(e.target.value)}
                        />

                        <Select
                            options={[
                                { value: 'Ativo', label: t('status.active') },
                                { value: 'Inativo', label: t('status.inactive') },
                                { value: 'Todos', label: t('status.all') },
                            ]}
                            placeholder={t('status.placeholder')}
                            style={{ width: 140 }}
                            value={statusFilter}
                            onChange={setStatusFilter}
                        />

                        <Flex gap="small" style={{ marginLeft: 'auto' }}>
                            <Button onClick={() => { setSearchText(''); setStatusFilter('Ativo'); }}>Limpar</Button>
                        </Flex>
                    </Flex>
                </Flex>

                <div style={{ background: '#fff', padding: '24px', borderRadius: '8px' }}>
                    <Flex vertical gap="middle" style={{ width: '100%' }}>
                        <Flex justify="space-between" align="center">
                            <span>Total: {filteredData.length} linhas</span>
                            <Button type="primary" onClick={onNavigateToForm}>Adicionar Linha</Button>
                        </Flex>

                        <Form form={form} component={false}>
                            <Table
                                components={{
                                    body: {
                                        cell: EditableCell,
                                    },
                                }}
                                bordered
                                dataSource={filteredData}
                                columns={mergedColumns}
                                rowClassName="editable-row"
                            />
                        </Form>
                    </Flex>
                </div>
            </Flex>
        </Spin>
    );
}
