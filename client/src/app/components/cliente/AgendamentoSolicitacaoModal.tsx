import { useEffect } from 'react';
import { Alert, DatePicker, Flex, Form, Modal, Select, Typography } from 'antd';
import dayjs, { type Dayjs } from 'dayjs';
import { CalendarOutlined, ShopOutlined } from '@ant-design/icons';
import { t } from '@/app/i18n';
import { Loja } from '@/app/models/Loja';

const { Text } = Typography;

export interface AgendamentoSolicitacaoItem {
  marca: string;
  modelo: string;
  numeroRevisao: number;
  dataMinima: string;
  dataLimite: string;
  dataIdeal?: string;
  dataAgendada?: string | null;
}

interface AgendamentoSolicitacaoModalProps {
  item: AgendamentoSolicitacaoItem | null;
  lojas: Loja[];
  open: boolean;
  confirming: boolean;
  mode: 'agendar' | 'reagendar';
  onConfirm: (values: { lojaId: number; dataAgendada: string }) => Promise<void>;
  onCancel: () => void;
}

export default function AgendamentoSolicitacaoModal({
  item,
  lojas,
  open,
  confirming,
  mode,
  onConfirm,
  onCancel,
}: AgendamentoSolicitacaoModalProps) {
  const [form] = Form.useForm<{ lojaId: number; dataAgendada: Dayjs }>();

  const getInitialDate = (currentItem: AgendamentoSolicitacaoItem) => {
    const hoje = dayjs().startOf('day');
    const minima = dayjs(currentItem.dataMinima.substring(0, 10)).startOf('day');
    const limite = dayjs(currentItem.dataLimite.substring(0, 10)).startOf('day');
    const candidates = [
      currentItem.dataAgendada,
      currentItem.dataIdeal,
      hoje.format('YYYY-MM-DD'),
    ].filter(Boolean) as string[];

    return candidates
      .map((date) => dayjs(date.substring(0, 10)).startOf('day'))
      .find((date) => !date.isBefore(hoje) && !date.isBefore(minima) && !date.isAfter(limite));
  };

  useEffect(() => {
    if (!open) {
      form.resetFields();
      return;
    }

    form.setFieldsValue({
      dataAgendada: item ? getInitialDate(item) : undefined,
    });
  }, [form, item, open]);

  const disabledDate = (date: Dayjs) => {
    if (!item) return true;

    const data = date.startOf('day');
    const hoje = dayjs().startOf('day');
    const minima = dayjs(item.dataMinima.substring(0, 10)).startOf('day');
    const limite = dayjs(item.dataLimite.substring(0, 10)).endOf('day');

    return data.isBefore(hoje) || data.isBefore(minima) || data.isAfter(limite);
  };

  const handleOk = async () => {
    const values = await form.validateFields();
    await onConfirm({
      lojaId: values.lojaId,
      dataAgendada: values.dataAgendada.format('YYYY-MM-DD'),
    });
  };

  const handleCancel = () => {
    form.resetFields();
    onCancel();
  };

  const titleKey = mode === 'reagendar'
    ? 'clienteAgendamentos.scheduleModal.rescheduleTitle'
    : 'clienteAgendamentos.scheduleModal.title';

  return (
    <Modal
      title={t(titleKey)}
      open={open}
      onOk={handleOk}
      onCancel={handleCancel}
      okText={t('clienteAgendamentos.scheduleModal.ok')}
      cancelText={t('clienteAgendamentos.scheduleModal.cancel')}
      confirmLoading={confirming}
      destroyOnHidden
    >
      {item && (
        <Flex vertical gap="middle">
          <Alert
            type="info"
            showIcon
            message={t('clienteAgendamentos.scheduleModal.alert.title', {
              motorcycle: `${item.marca} ${item.modelo}`,
              revision: item.numeroRevisao,
            })}
            description={t('clienteAgendamentos.scheduleModal.alert.description', {
              start: dayjs(item.dataMinima.substring(0, 10)).format('DD/MM/YYYY'),
              end: dayjs(item.dataLimite.substring(0, 10)).format('DD/MM/YYYY'),
            })}
          />

          <Form form={form} layout="vertical">
            <Form.Item
              label={t('clienteAgendamentos.scheduleModal.shop.label')}
              name="lojaId"
              rules={[{ required: true, message: t('clienteAgendamentos.scheduleModal.shop.required') }]}
            >
              <Select
                showSearch
                optionFilterProp="label"
                placeholder={t('clienteAgendamentos.scheduleModal.shop.placeholder')}
                options={lojas.map((loja) => ({
                  value: loja.id,
                  label: `${loja.nome} - ${loja.cidade}/${loja.uf}`,
                }))}
                suffixIcon={<ShopOutlined />}
              />
            </Form.Item>

            <Form.Item
              label={t('clienteAgendamentos.scheduleModal.date.label')}
              name="dataAgendada"
              rules={[{ required: true, message: t('clienteAgendamentos.scheduleModal.date.required') }]}
            >
              <DatePicker
                style={{ width: '100%' }}
                format="DD/MM/YYYY"
                disabledDate={disabledDate}
                placeholder={t('clienteAgendamentos.scheduleModal.date.placeholder')}
                suffixIcon={<CalendarOutlined />}
              />
            </Form.Item>
          </Form>

          <Text type="secondary">
            {t('clienteAgendamentos.scheduleModal.footer')}
          </Text>
        </Flex>
      )}
    </Modal>
  );
}
