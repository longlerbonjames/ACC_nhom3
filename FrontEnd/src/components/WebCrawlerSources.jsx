// src/components/CrawlerSelectorTable.jsx
import React, { useEffect, useState } from 'react';
import { Table, Button, Space, Modal, Form, Input, message } from 'antd';
import axios from 'axios';

const defaultValues = {
  url: '',
  listContainerClass: '',
  titleSelector: 'div/a',
  imageSelector: 'img',
  detailContainerClass: 'item-content-detail',
  labelClass: 'item-content-p',
  priceClass: 'price-new',
};

const CrawlerSelectorTable = () => {
  const [data, setData] = useState([]);
  const [form] = Form.useForm();
  const [editingItem, setEditingItem] = useState(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  const fetchData = async () => {
    try {
      const res = await axios.get('/api/crawler-selectors');
      setData(res.data);
    } catch (err) {
      console.error(err);
      message.error('Failed to fetch data');
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleDelete = async (id) => {
    try {
      await axios.delete(`/api/crawler-selectors/${id}`);
      message.success('Deleted');
      fetchData();
    } catch {
      message.error('Delete failed');
    }
  };

  const handleEdit = (record) => {
    form.setFieldsValue(record);
    setEditingItem(record);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    form.resetFields();
    setEditingItem(null);
    setIsModalOpen(true);
  };

  const handleFinish = async (values) => {
    try {
      if (editingItem) {
        await axios.put(`/api/crawler-selectors/${editingItem.id}`, values);
        message.success('Updated successfully');
      } else {
        await axios.post('/api/crawler-selectors', values);
        message.success('Created successfully');
      }
      setIsModalOpen(false);
      fetchData();
    } catch {
      message.error('Save failed');
    }
  };

  const columns = [
    { title: 'Url', dataIndex: 'url', key: 'url' },
    { title: 'ListContainerClass', dataIndex: 'listContainerClass', key: 'listContainerClass' },
    { title: 'TitleSelector', dataIndex: 'titleSelector', key: 'titleSelector' },
    { title: 'ImageSelector', dataIndex: 'imageSelector', key: 'imageSelector' },
    { title: 'DetailContainerClass', dataIndex: 'detailContainerClass', key: 'detailContainerClass' },
    { title: 'LabelClass', dataIndex: 'labelClass', key: 'labelClass' },
    { title: 'PriceClass', dataIndex: 'priceClass', key: 'priceClass' },
    {
      title: 'Actions',
      render: (_, record) => (
        <Space>
          <Button onClick={() => handleEdit(record)} type="link">Edit</Button>
          <Button onClick={() => handleDelete(record.id)} type="link" danger>Delete</Button>
        </Space>
      ),
    },
  ];

  return (
    <div style={{ padding: 24 }}>
      <Button type="primary" onClick={handleCreate} style={{ marginBottom: 16 }}>
        Add New
      </Button>
      <Table rowKey="id" dataSource={data} columns={columns} />

      <Modal
        title={editingItem ? 'Edit Selector' : 'Create Selector'}
        open={isModalOpen}
        onCancel={() => setIsModalOpen(false)}
        onOk={() => form.submit()}
      >
        <Form form={form} layout="vertical" onFinish={handleFinish} initialValues={defaultValues}>
          <Form.Item name="url" label="Url" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="listContainerClass" label="List Container Class" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="titleSelector" label="Title Selector">
            <Input />
          </Form.Item>
          <Form.Item name="imageSelector" label="Image Selector">
            <Input />
          </Form.Item>
          <Form.Item name="detailContainerClass" label="Detail Container Class">
            <Input />
          </Form.Item>
          <Form.Item name="labelClass" label="Label Class">
            <Input />
          </Form.Item>
          <Form.Item name="priceClass" label="Price Class">
            <Input />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default CrawlerSelectorTable;
