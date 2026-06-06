import { useState } from 'react';
import { Breadcrumb, Typography, Form, Input, Button, Flex, Spin, message } from 'antd';
import { HomeOutlined, ToolOutlined, ArrowLeftOutlined } from '@ant-design/icons';
import { linhaService } from '@/app/services/linhaService';
import { handleApiError } from '@/app/utils/errorHandler';
import { t } from '@/app/i18n';

const { Title } = Typography;

interface CatalogoLinhasCreateProps {
    onCancel?: () => void;
}

export default function CatalogoLinhasCreate({ onCancel }: CatalogoLinhasCreateProps) {
    const [form] = Form.useForm();
    const [loading, setLoading] = useState(false);

    const handleSubmit = async (values: any) => {
        try {
            setLoading(true);
            await linhaService.create({
                nome: values.nome,
                descricao: values.descricao,
            });
            message.success(t('linhaCreatedSuccess'));
            onCancel?.();
        } catch (error) {
            handleApiError(error);
        } finally {
            setLoading(false);
        }
    };

    const handleCancel = () => {
        form.resetFields();
        onCancel?.();
    };

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
                            {
                                title: 'Adicionar Linha',
                            },
                        ]}
                    />

                    <Flex align="center" gap="middle">
                        <Button
                            type="default"
                            icon={<ArrowLeftOutlined />}
                            onClick={onCancel}
                        />
                        <Title level={2} style={{ margin: 0 }}>
                            Adicionar Linha
                        </Title>
                    </Flex>
                </Flex>

                <div style={{ background: '#fff', padding: '24px', borderRadius: '8px' }}>
                    <Form
                        form={form}
                        layout="vertical"
                        onFinish={handleSubmit}
                        style={{ maxWidth: '800px' }}
                    >
                        <Form.Item
                            label="Nome da Linha"
                            name="nome"
                            rules={[{ required: true, message: 'Por favor, insira o nome da linha' }]}
                        >
                            <Input placeholder="Ex: Passeio, Trail, Esportiva" />
                        </Form.Item>

                        <Form.Item
                            label="Descrição"
                            name="descricao"
                            rules={[{ required: true, message: 'Por favor, insira a descrição' }]}
                        >
                            <Input.TextArea
                                placeholder="Ex: Motos para uso urbano e estradas"
                                rows={3}
                            />
                        </Form.Item>

                        <Form.Item>
                            <Flex gap="middle">
                                <Button type="primary" htmlType="submit" size="large">
                                    Salvar
                                </Button>
                                <Button size="large" onClick={handleCancel}>
                                    Cancelar
                                </Button>
                            </Flex>
                        </Form.Item>
                    </Form>
                </div>
            </Flex>
        </Spin>
    );
}
