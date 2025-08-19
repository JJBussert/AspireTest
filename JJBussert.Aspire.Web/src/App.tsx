import React, { useState, useEffect } from 'react';
import './App.css';

interface User {
  id: number;
  name: string;
  email: string;
  role: string;
  organizationId: number;
  organization: Organization;
  createdAt: string;
}

interface Organization {
  id: number;
  name: string;
  description?: string;
  createdAt: string;
}

interface ClientPrincipal {
  identityProvider: string;
  userId: string;
  userDetails: string;
  userRoles: string[];
}

function App() {
  const [users, setUsers] = useState<User[]>([]);
  const [organizations, setOrganizations] = useState<Organization[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [clientPrincipal, setClientPrincipal] = useState<ClientPrincipal | null>(null);
  const [authLoading, setAuthLoading] = useState(true);

  useEffect(() => {
    const fetchAuth = async () => {
      try {
        const response = await fetch('/.auth/me');
        if (response.ok) {
          const authData = await response.json();
          setClientPrincipal(authData.clientPrincipal);
        }
      } catch (err) {
        console.log('Not authenticated or auth endpoint not available');
      } finally {
        setAuthLoading(false);
      }
    };

    fetchAuth();
  }, []);

  useEffect(() => {
    if (authLoading) return;

    const fetchData = async () => {
      try {
        setLoading(true);

        // Get API base URL from environment or use service discovery
        const apiBaseUrl = process.env.REACT_APP_API_URL || '/api';

        const [usersResponse, orgsResponse] = await Promise.all([
          fetch(`${apiBaseUrl}/users`),
          fetch(`${apiBaseUrl}/organizations`)
        ]);

        if (!usersResponse.ok || !orgsResponse.ok) {
          if (usersResponse.status === 401 || orgsResponse.status === 401) {
            setError('Authentication required. Please log in.');
            return;
          }
          throw new Error('Failed to fetch data');
        }

        const usersData = await usersResponse.json();
        const orgsData = await orgsResponse.json();

        setUsers(usersData);
        setOrganizations(orgsData);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'An error occurred');
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [authLoading]);

  if (authLoading || loading) {
    return <div className="App">Loading...</div>;
  }

  if (error) {
    return (
      <div className="App">
        <div className="error-container">
          <h2>Error: {error}</h2>
          {error.includes('Authentication required') && (
            <div>
              <p>Please log in to access this application.</p>
              <a href="/.auth/login/aad" className="login-button">Login</a>
            </div>
          )}
        </div>
      </div>
    );
  }

  return (
    <div className="App">
      <header className="App-header">
        <h1>JJBussert Aspire Application</h1>
        <div className="user-info">
          {clientPrincipal ? (
            <div>
              <span>Welcome, {clientPrincipal.userDetails}</span>
              <span className={`role-badge ${clientPrincipal.userRoles?.[0]?.toLowerCase()}`}>
                {clientPrincipal.userRoles?.[0]}
              </span>
              <a href="/.auth/logout" className="logout-button">Logout</a>
            </div>
          ) : (
            <a href="/.auth/login/aad" className="login-button">Login</a>
          )}
        </div>
        <p>Organizations: {organizations.length} | Users: {users.length}</p>
      </header>
      
      <main>
        <section>
          <h2>Organizations</h2>
          <div className="organizations-grid">
            {organizations.map(org => (
              <div key={org.id} className="organization-card">
                <h3>{org.name}</h3>
                <p>{org.description}</p>
                <small>Created: {new Date(org.createdAt).toLocaleDateString()}</small>
              </div>
            ))}
          </div>
        </section>

        <section>
          <h2>Users</h2>
          <div className="users-table">
            <table>
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Email</th>
                  <th>Role</th>
                  <th>Organization</th>
                  <th>Created</th>
                </tr>
              </thead>
              <tbody>
                {users.map(user => (
                  <tr key={user.id}>
                    <td>{user.name}</td>
                    <td>{user.email}</td>
                    <td><span className={`role-badge ${user.role.toLowerCase()}`}>{user.role}</span></td>
                    <td>{user.organization?.name}</td>
                    <td>{new Date(user.createdAt).toLocaleDateString()}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>
      </main>
    </div>
  );
}

export default App;
