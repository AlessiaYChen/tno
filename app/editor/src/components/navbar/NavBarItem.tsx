import { useNavigate } from 'react-router';
import { Claim, useKeycloakWrapper } from 'tno-core';

import * as styled from './styled';

export interface INavBarItemProps extends React.HTMLProps<HTMLButtonElement> {
  /**
   * choose the tab label
   */
  label?: string;
  /**
   * prop used to determine whether the tab is active
   */
  active?: boolean;
  /**
   * the path the item will navigate you to
   */
  navigateTo?: string;
  /**
   * The user requires at least one of the specified claims to see this nav item.
   */
  claim?: Claim | Claim[];
}

/**
 * The individual item that will appear in the navigation bar, on click it will navigate to desired path and will use the applications
 * current path to determine whether it is active or not.
 * @param label the text to appear on the tab in the navigation bar
 * @param navigateTo determine the path the item will navigate to onClick
 * @returns styled navigation bar item
 */
export const NavBarItem: React.FC<INavBarItemProps> = ({ label, navigateTo, claim }) => {
  const navigate = useNavigate();
  const keycloak = useKeycloakWrapper();
  const hasClaim = !claim || keycloak.hasClaim(claim);

  let path = window.location.pathname;
  let isActive = navigateTo ? path.startsWith(navigateTo) : false;

  return hasClaim ? (
    <styled.NavBarItem onClick={() => navigate(navigateTo!!)} active={isActive}>
      {label}
    </styled.NavBarItem>
  ) : null;
};
