import React from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';

import {
  CBRAReport,
  ContentLogs,
  ContentReferenceList,
  DataSource,
  DataSourceDetails,
  DataSourceList,
  MediaType,
  MediaTypeList,
  ReachEarnedMedia,
  ServiceConfig,
  UserList,
} from '.';

export const AdminRouter: React.FC = () => {
  return (
    <Routes>
      <Route index element={<Navigate to="data/sources" />} />
      <Route path="users" element={<UserList />} />
      <Route path="media/types/:id" element={<MediaType />} />
      <Route path="media/types" element={<MediaTypeList />} />
      <Route path="data/sources" element={<DataSourceList />} />
      <Route path="data/sources/:id" element={<DataSource />}>
        <Route index element={<DataSourceDetails />} />
        <Route path="details" element={<DataSourceDetails />} />
        <Route path="service" element={<ServiceConfig />} />
        <Route path="metrics" element={<ReachEarnedMedia />} />
        <Route path="ingesting" element={<ContentReferenceList />} />
      </Route>
      <Route path="contents/log" element={<ContentLogs />} />
      <Route path="reports/cbra" element={<CBRAReport />} />
    </Routes>
  );
};