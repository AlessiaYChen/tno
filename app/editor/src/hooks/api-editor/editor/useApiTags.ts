import { defaultEnvelope, ILifecycleToasts } from 'tno-core';

import { ITagModel, useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiTags = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return {
    getTags: (etag: string | undefined = undefined) => {
      const config = !!etag ? { headers: { 'If-None-Match': etag } } : undefined;
      return api.get<ITagModel[]>(`/editor/tags`, config);
    },
  };
};
