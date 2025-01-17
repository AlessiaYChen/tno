import { useKeycloak } from '@react-keycloak/web';
import React from 'react';

import { Claim, IKeycloak, IKeycloakToken, Role } from '.';

let token: IKeycloakToken = { client_roles: [], groups: [] };

/**
 * Provides extension methods to interact with the `keycloak` object.
 */
export function useKeycloakWrapper(): IKeycloak {
  const { initialized: ready, keycloak } = useKeycloak();

  const [initialized, setInitialized] = React.useState(ready);

  React.useEffect(() => {
    // For some reason the keycloak value doesn't update state.
    if (initialized) token = keycloak.tokenParsed as IKeycloakToken;
    setInitialized(initialized && !!keycloak.authenticated);
  }, [keycloak, initialized, keycloak.authenticated]);

  const controller = React.useMemo(
    () => ({
      /**
       * Determine if the user has the specified 'claim', or one of the specified 'claims'.
       * @param claim - The name of the claim
       */
      hasClaim: (claim?: Claim | Array<Claim>): boolean => {
        if (claim === undefined && !!token?.client_roles?.length) return true;
        return (
          claim !== undefined &&
          claim !== null &&
          (typeof claim === 'string'
            ? token?.client_roles?.includes(claim) ?? false
            : claim.some((c) => token?.client_roles?.includes(c)))
        );
      },
      /**
       * Determine if the user belongs to the specified 'role', or one of the specified 'roles'.
       * @param role - The role name or an array of role name
       */
      hasRole: (role?: Role | Array<Role>): boolean => {
        if (role === undefined && !!token?.groups?.length) return true;
        return (
          role !== undefined &&
          role !== null &&
          (typeof role === 'string'
            ? token?.groups?.includes(role) ?? false
            : role.some((r) => token?.groups?.includes(r)))
        );
      },
      /**
       * Extract the display name from the token.
       * @returns User's display name.
       */
      getDisplayName: () => {
        return token?.display_name ?? '';
      },
      /**
       * Extract the unique username of the user
       * @returns User's unique username
       */
      getUserKey: () => {
        return token?.preferred_username ?? '';
      },
      /**
       * Extract the unique username of the user
       * @returns User's unique username
       */
      getUsername: () => {
        return (
          token?.username ??
          token?.idir_username ??
          token?.github_username ??
          token?.bceid_username ??
          ''
        );
      },
      /**
       * Extract the unique UID of the user
       * @returns User's unique username
       */
      getUserUid: () => {
        return token?.idir_user_guid ?? token?.github_id ?? token?.bceid_user_guid ?? '';
      },
    }),
    [],
  );

  return {
    initialized,
    instance: keycloak,
    authenticated: keycloak.authenticated,
    ...controller,
  };
}
