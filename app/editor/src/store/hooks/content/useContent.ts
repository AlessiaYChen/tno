import { IContentFilter, IContentModel, IPaged } from 'hooks/api-editor';
import { useApiContents } from 'hooks/api-editor';
import React from 'react';
import { IContentStore, useContentStore } from 'store/slices';
import { IContentProps, IContentState } from 'store/slices/content';

interface IContentHook {
  getContent: (id: number) => Promise<IContentModel>;
  findContent: (filter: IContentFilter) => Promise<IPaged<IContentModel>>;
  addContent: (content: IContentModel) => Promise<IContentModel>;
  updateContent: (content: IContentModel) => Promise<IContentModel>;
  deleteContent: (content: IContentModel) => Promise<IContentModel>;
}

export const useContent = (props?: IContentProps): [IContentState, IContentHook, IContentStore] => {
  const [state, store] = useContentStore(props);
  const api = useApiContents();

  const hook: IContentHook = React.useMemo(
    () => ({
      getContent: async (id: number) => {
        const result = await api.getContent(id);
        return result;
      },
      findContent: async (filter: IContentFilter) => {
        const result = await api.findContent(filter);
        return result;
      },
      addContent: async (content: IContentModel) => {
        const result = await api.addContent(content);
        return result;
      },
      updateContent: async (content: IContentModel) => {
        const result = await api.updateContent(content);
        return result;
      },
      deleteContent: async (content: IContentModel) => {
        const result = await api.deleteContent(content);
        return result;
      },
    }),
    [api],
  );

  return [state, hook, store];
};
