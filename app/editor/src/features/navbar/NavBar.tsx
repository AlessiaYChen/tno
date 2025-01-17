import React from 'react';
import { Claim, NavBarGroup, NavBarItem, Show } from 'tno-core';
import { Row } from 'tno-core/dist/components/flex/row';

/**
 * The navigation bar that is used throughout the TNO editor application. Add or remove navigation bar items here.
 */
export const NavBar: React.FC = () => {
  const [activeHover, setActiveHover] = React.useState<string>('');

  const hideRef = React.useRef(false);
  const ref = React.useRef<any>();

  React.useEffect(() => {
    document.addEventListener('click', handleClickOutside, false);
    return () => {
      document.removeEventListener('click', handleClickOutside, false);
    };
  });

  const onMouseOver = () => {
    hideRef.current = false;
  };

  const onMouseLeave = () => {
    hideRef.current = true;
    setTimeout(() => {
      if (hideRef.current) setActiveHover('');
    }, 2000);
  };

  const handleClickOutside = (event: { target: any }) => {
    if (ref.current && !ref.current.contains(event.target)) {
      hideRef.current = true;
      setActiveHover('');
    }
  };

  const handleClick = (menu: string = '') => {
    if (activeHover === menu) setActiveHover('');
    else setActiveHover(menu);
  };

  return (
    <div onMouseLeave={onMouseLeave} onMouseOver={onMouseOver} ref={ref}>
      <NavBarGroup className="navbar">
        <Row>
          <div className="editor" onClick={() => handleClick('editor')}>
            <NavBarItem activeHoverTab={activeHover} label="Editor" claim={Claim.editor} />
          </div>
          <div className="admin" onClick={() => handleClick('admin')}>
            <NavBarItem activeHoverTab={activeHover} label="Admin" claim={Claim.administrator} />
          </div>
          <div className="report" onClick={() => handleClick('report')}>
            <NavBarItem activeHoverTab={activeHover} label="Reports" claim={Claim.administrator} />
          </div>
        </Row>
      </NavBarGroup>
      <NavBarGroup hover className="navbar" onClick={() => handleClick()}>
        <Row hidden={!activeHover}>
          {/* Editor */}
          <Show visible={activeHover === 'editor'}>
            <NavBarItem navigateTo="/contents" label="Content" claim={Claim.editor} />
            <NavBarItem navigateTo="/morning/reports" label="Morning Report" claim={Claim.editor} />
            <NavBarItem navigateTo="/storage" label="Storage" claim={Claim.editor} />
            <NavBarItem
              navigateTo="/contents/log"
              label="Linked Snippet Log"
              claim={Claim.administrator}
            />
          </Show>

          {/* Admin */}
          <Show visible={activeHover === 'admin'}>
            <NavBarItem
              navigateTo="/admin/categories"
              label="Categories"
              claim={Claim.administrator}
            />
            <NavBarItem navigateTo="/admin/tags" label="Tags" claim={Claim.administrator} />
            <NavBarItem navigateTo="/admin/series" label="Series" claim={Claim.administrator} />
            <NavBarItem navigateTo="/admin/users" label="Users" claim={Claim.administrator} />
            <NavBarItem navigateTo="/admin/sources" label="Sources" claim={Claim.administrator} />
            <NavBarItem navigateTo="/admin/products" label="Products" claim={Claim.administrator} />
            <NavBarItem navigateTo="/admin/licences" label="Licences" claim={Claim.administrator} />
            <NavBarItem navigateTo="/admin/actions" label="Actions" claim={Claim.administrator} />
            <NavBarItem
              navigateTo="/admin/connections"
              label="Connections"
              claim={Claim.administrator}
            />
            <NavBarItem
              navigateTo="/admin/data/locations"
              label="Data Locations"
              claim={Claim.administrator}
            />
            <NavBarItem navigateTo="/admin/ingests" label="Ingest" claim={Claim.administrator} />
            <NavBarItem
              navigateTo="/admin/ingest/types"
              label="Ingest Types"
              claim={Claim.administrator}
            />
            <NavBarItem
              navigateTo="/admin/work/orders"
              label="Work Orders"
              claim={Claim.administrator}
            />
          </Show>

          {/* Reports */}
          <Show visible={activeHover === 'report'}>
            <NavBarItem
              navigateTo="/reports/cbra"
              label="CBRA Report"
              claim={Claim.administrator}
            />
          </Show>
        </Row>
      </NavBarGroup>
    </div>
  );
};
