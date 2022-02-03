import { ContentForm, ContentListView, Login } from 'features';
import { Route, Routes } from 'react-router-dom';
import { Claim, NotFound } from 'tno-core';

import { PrivateRoute } from '.';

/**
 * AppRouter provides a SPA router to manage routes.
 * @returns AppRouter component.
 */
export const AppRouter = () => {
  return (
    <Routes>
      <Route path="/" element={<p>LIST VIEW GOES HERE :)</p>} />
      <Route path="/login" element={<Login />} />
      {/* <Route path="/snippetview" element={<SnippetView />} /> */}
      <Route
        path="/admin"
        element={
          <PrivateRoute redirectTo="/login" claims={Claim.administrator}>
            <p>Administration</p>
          </PrivateRoute>
        }
      />
      <Route
        path="/contents"
        element={
          <PrivateRoute
            redirectTo="/login"
            claims={Claim.editor}
            element={<ContentListView />}
          ></PrivateRoute>
        }
      />
      <Route
        path="/contents/:id"
        element={
          <PrivateRoute
            redirectTo="/login"
            claims={Claim.editor}
            element={<ContentForm />}
          ></PrivateRoute>
        }
      />
      <Route path="*" element={<NotFound />} />
    </Routes>
  );
};
