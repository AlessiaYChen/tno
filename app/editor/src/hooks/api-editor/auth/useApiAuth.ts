import React from 'react';
import { defaultEnvelope, extractResponseData, ILifecycleToasts } from 'tno-core';

import { IUserInfoModel, useApi } from '..';

/**
 * Common hook to make requests to the PIMS APi.
 * @returns CustomAxios object setup for the PIMS API.
 */
export const useApiAuth = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    getUserInfo: () => {
      return extractResponseData<IUserInfoModel>(() => api.get(`/auth/userinfo`));
    },
  }).current;
};
