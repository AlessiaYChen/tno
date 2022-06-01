import { IconButton, OptionItem } from 'components/form';
import {
  FormikCheckbox,
  FormikForm,
  FormikSelect,
  FormikText,
  FormikTextArea,
} from 'components/formik';
import { Modal } from 'components/modal';
import { useModal } from 'hooks';
import { IUserModel, UserStatusName } from 'hooks/api-editor';
import React from 'react';
import { FaTrash } from 'react-icons/fa';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import ReactTooltip from 'react-tooltip';
import { useLookup } from 'store/hooks';
import { useUsers } from 'store/hooks/admin';
import { Button, ButtonVariant, Show } from 'tno-core';
import { Col, Row } from 'tno-core';

import { defaultUser } from './constants';
import * as styled from './styled';

/** The page used to view and edit media types in the administrative section. */
export const UserForm: React.FC = () => {
  const [, api] = useUsers();
  const { id } = useParams();
  const navigate = useNavigate();
  const userId = Number(id);
  const { toggle, isShowing } = useModal();
  const [lookups] = useLookup();

  const [user, setUser] = React.useState<IUserModel>(defaultUser);
  const [roleOptions, setRoleOptions] = React.useState(
    lookups.roles.map((r) => new OptionItem(r.name, r.id)),
  );

  const isLinkedToKeycloak = user.key !== '00000000-0000-0000-0000-000000000000';
  const statusOptions = [
    UserStatusName.Requested,
    UserStatusName.Approved,
    UserStatusName.Denied,
  ].map((s) => new OptionItem(s, s));

  React.useEffect(() => {
    if (!!userId && user?.id !== userId) {
      api.getUser(userId).then((data) => {
        setUser(data);
      });
    }
  }, [api, user?.id, userId]);

  React.useEffect(() => {
    setRoleOptions(lookups.roles.map((r) => new OptionItem(r.name, r.id)));
  }, [lookups.roles]);

  React.useEffect(() => {
    ReactTooltip.rebuild();
  });

  const handleSubmit = async (values: IUserModel) => {
    try {
      const originalId = values.id;
      const result = !user.id ? await api.addUser(values) : await api.updateUser(values);
      setUser(result);
      toast.success(`${result.username} has successfully been saved.`);
      if (!originalId) navigate(`/admin/users/${result.id}`);
    } catch {}
  };

  return (
    <styled.UserForm>
      <IconButton
        iconType="back"
        label="Back to Users"
        className="back-button"
        onClick={() => navigate('/admin/users')}
      />
      <FormikForm
        initialValues={user}
        onSubmit={(values, { setSubmitting }) => {
          handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ values, isSubmitting, setFieldValue }) => (
          <div className="form-container">
            <Row>
              <Col className="form-inputs">
                <FormikText name="username" label="Username" disabled={isLinkedToKeycloak} />
              </Col>
              <Col
                className="form-inputs"
                onClick={(e) => {
                  if (e.ctrlKey) setUser({ ...user, status: UserStatusName.Requested });
                }}
              >
                <Show visible={user.status === UserStatusName.Requested}>
                  <FormikSelect name="status" label="Status" options={statusOptions} />
                </Show>
                <Show visible={user.status !== UserStatusName.Requested}>
                  <FormikText name="status" label="Status" disabled />
                </Show>
              </Col>
            </Row>
            <FormikText name="email" label="Email" disabled={isLinkedToKeycloak} />
            <Row>
              <Col className="form-inputs">
                <FormikText
                  name="displayName"
                  label="Display Name"
                  tooltip="Friendly name to use instead of username"
                />
                <FormikCheckbox label="Email Verified" name="emailVerified" />
                <FormikCheckbox label="Is Enabled" name="isEnabled" />
              </Col>
              <Col className="form-inputs">
                <FormikText name="firstName" label="First Name" disabled={isLinkedToKeycloak} />
                <FormikText name="lastName" label="Last Name" disabled={isLinkedToKeycloak} />
              </Col>
            </Row>
            {!!user.id && (
              <FormikText name="key" label="Key" tooltip="Keycloak UID reference" disabled />
            )}
            <FormikTextArea name="note" label="Note" />
            <div className="roles">
              <Row className="form-inputs">
                <Col flex="1 1 auto">
                  <FormikSelect
                    label="Roles"
                    name="role"
                    options={roleOptions}
                    placeholder="Select Role"
                    tooltip="Add a role to the user"
                  >
                    <Button
                      variant={ButtonVariant.secondary}
                      disabled={!(values as any).role}
                      onClick={(e) => {
                        const id = (values as any).role;
                        const role = lookups.roles.find((r) => r.id === id);
                        if (role && !values.roles?.some((r) => r.id === id))
                          setUser({ ...values, roles: [...(values.roles ?? []), role] });
                      }}
                    >
                      Add
                    </Button>
                  </FormikSelect>
                </Col>
              </Row>
              <hr />
              {user.roles?.map((r) => (
                <Row alignContent="stretch" key={r.id}>
                  <Col flex="1 1 auto">{r.name}</Col>
                  <Col>
                    <Button
                      variant={ButtonVariant.danger}
                      onClick={(e) => {
                        setUser({
                          ...values,
                          roles: values.roles?.filter((ur) => ur.id !== r.id) ?? [],
                        });
                        setFieldValue('role', 0);
                      }}
                    >
                      <FaTrash />
                    </Button>
                  </Col>
                </Row>
              ))}
            </div>
            <Row justifyContent="center" className="form-inputs">
              <Button type="submit" disabled={isSubmitting}>
                Save
              </Button>
              <Button onClick={toggle} variant={ButtonVariant.danger} disabled={isSubmitting}>
                Delete
              </Button>
            </Row>
            <Modal
              headerText="Confirm Removal"
              body="Are you sure you wish to remove this user?"
              isShowing={isShowing}
              hide={toggle}
              type="delete"
              confirmText="Yes, Remove It"
              onConfirm={async () => {
                try {
                  await api.deleteUser(user);
                  toast.success(`${user.username} has successfully been deleted.`);
                  navigate('/admin/users');
                } finally {
                  toggle();
                }
              }}
            />
          </div>
        )}
      </FormikForm>
    </styled.UserForm>
  );
};
