import React, { useEffect, useState } from 'react';
import {
  Table,
  Typography,
  Image,
  Button,
  Popconfirm,
  message,
  Modal,
  Form,
  Input,
} from 'antd';
import axios from 'axios';

const { Title } = Typography;

const TourTable = () => {
  const [tours, setTours] = useState([]);
  const [loading, setLoading] = useState(false);
  const [editingTour, setEditingTour] = useState(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [form] = Form.useForm();

  const fetchTours = async () => {
    try {
      setLoading(true);
      const res = await axios.get('https://localhost:7215/api/Tours');
      setTours(res.data);
    } catch (err) {
      console.error('Lỗi khi fetch tour:', err.message);
      message.error('Không thể tải danh sách tour');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTours();
  }, []);

  const handleDelete = async (id) => {
    try {
      await axios.delete(`https://localhost:7215/api/Tours/${id}`);
      message.success('Đã xoá tour');
      fetchTours();
    } catch (err) {
      message.error('Lỗi khi xoá tour');
    }
  };

  const handleEdit = (tour) => {
    setEditingTour(tour);
    form.setFieldsValue(tour);
    setIsModalOpen(true);
  };

  const handleModalOk = async () => {
    try {
      const updatedTour = form.getFieldsValue();
      updatedTour.id = editingTour.id;
      await axios.put(`https://localhost:7215/api/Tours/${editingTour.id}`, updatedTour);
      message.success('Cập nhật thành công');
      setIsModalOpen(false);
      fetchTours();
    } catch (err) {
      message.error('Cập nhật thất bại');
    }
  };

  const handleModalCancel = () => {
    setIsModalOpen(false);
  };

  const columns = [
    {
      title: 'Ảnh',
      dataIndex: 'imageUrl',
      key: 'imageUrl',
      render: (url, record) => <Image src={url} alt={record.title} width={100} />,
    },
    {
      title: 'Tiêu đề',
      dataIndex: 'title',
      key: 'title',
      render: (text, record) => (
        <a href={record.url} target="_blank" rel="noreferrer">
          {text}
        </a>
      ),
    },
    {
      title: 'URL',
      dataIndex: 'url',
      key: 'url',
    },
    {
      title: 'Khởi hành từ',
      dataIndex: 'departurePoint',
      key: 'departurePoint',
    },
    {
      title: 'Điểm đến',
      dataIndex: 'destination',
      key: 'destination',
    },
    {
      title: 'Thời gian',
      dataIndex: 'duration',
      key: 'duration',
    },
    {
      title: 'Ngày khởi hành',
      dataIndex: 'departureTime',
      key: 'departureTime',
    },
    {
      title: 'Phương tiện',
      dataIndex: 'transportation',
      key: 'transportation',
    },
    {
      title: 'Giá',
      dataIndex: 'price',
      key: 'price',
    },
    {
      title: 'Nguồn',
      dataIndex: 'source',
      key: 'source',
    },
    {
      title: 'Thời gian crawl',
      dataIndex: 'crawledTime',
      key: 'crawledTime',
      render: (text) => new Date(text).toLocaleString(),
    },
    {
      title: 'Hành động',
      key: 'actions',
      render: (_, record) => (
        <div style={{ display: 'flex', gap: '8px' }}>
          <Button onClick={() => handleEdit(record)} type="primary" size="small">
            Sửa
          </Button>
          <Popconfirm
            title="Bạn có chắc chắn muốn xoá?"
            onConfirm={() => handleDelete(record.id)}
            okText="Xoá"
            cancelText="Huỷ"
          >
            <Button danger size="small">Xoá</Button>
          </Popconfirm>
        </div>
      ),
    },
  ];

  return (
    <div style={{ width: '100%', overflowX: 'auto'}}>
      <Title level={3}>Danh sách tour đã crawl</Title>
     <Table
    dataSource={tours}
    columns={columns}
    rowKey="id"
    loading={loading}
    pagination={{ pageSize: 10 }}
  />

      {/* Modal sửa tour */}
      <Modal
        title="Sửa thông tin tour"
        open={isModalOpen}
        onOk={handleModalOk}
        onCancel={handleModalCancel}
        okText="Lưu"
        cancelText="Huỷ"
      >
        <Form form={form} layout="vertical">
          <Form.Item label="Tiêu đề" name="title">
            <Input />
          </Form.Item>
          <Form.Item label="URL" name="url">
            <Input />
          </Form.Item>
          <Form.Item label="Ảnh" name="imageUrl">
            <Input />
          </Form.Item>
          <Form.Item label="Khởi hành từ" name="departurePoint">
            <Input />
          </Form.Item>
          <Form.Item label="Điểm đến" name="destination">
            <Input />
          </Form.Item>
          <Form.Item label="Thời gian" name="duration">
            <Input />
          </Form.Item>
          <Form.Item label="Ngày khởi hành" name="departureTime">
            <Input />
          </Form.Item>
          <Form.Item label="Phương tiện" name="transportation">
            <Input />
          </Form.Item>
          <Form.Item label="Giá" name="price">
            <Input />
          </Form.Item>
          <Form.Item label="Nguồn" name="source">
            <Input />
          </Form.Item>
          <Form.Item label="Thời gian crawl" name="crawledTime">
            <Input disabled />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default TourTable;
