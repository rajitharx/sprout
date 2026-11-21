import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { api, ApiError } from './client';

function respond(body: unknown, init: ResponseInit = { status: 200 }) {
  return Promise.resolve(new Response(JSON.stringify(body), init));
}

describe('api client', () => {
  const fetchMock = vi.fn();

  beforeEach(() => {
    vi.stubGlobal('fetch', fetchMock);
  });

  afterEach(() => {
    fetchMock.mockReset();
    vi.unstubAllGlobals();
    vi.useRealTimers();
  });

  it('getTasks returns parsed tasks', async () => {
    const tasks = [{ id: '1', label: 'Brush', emoji: '🪥', sortOrder: 0, isActive: true }];
    fetchMock.mockReturnValueOnce(respond(tasks));

    await expect(api.getTasks()).resolves.toEqual(tasks);
    expect(fetchMock.mock.calls[0][0]).toBe('/api/tasks');
  });

  it('createTask posts the task as JSON', async () => {
    const created = { id: 'n', label: 'Bath', emoji: '🛁', sortOrder: 1, isActive: true };
    fetchMock.mockReturnValueOnce(respond(created));

    await api.createTask({ label: 'Bath', emoji: '🛁', sortOrder: 1, isActive: true });

    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe('/api/tasks');
    expect(init.method).toBe('POST');
    expect(init.headers).toEqual({ 'Content-Type': 'application/json' });
    expect(JSON.parse(init.body).label).toBe('Bath');
  });

  it('updateTask puts to the task URL', async () => {
    const task = { id: 'abc', label: 'X', emoji: '✅', sortOrder: 0, isActive: true };
    fetchMock.mockReturnValueOnce(respond(task));

    await api.updateTask('abc', task);

    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe('/api/tasks/abc');
    expect(init.method).toBe('PUT');
  });

  it('deleteTask resolves on 204', async () => {
    fetchMock.mockReturnValueOnce(Promise.resolve(new Response(null, { status: 204 })));

    await expect(api.deleteTask('abc')).resolves.toBeUndefined();
    expect(fetchMock.mock.calls[0][1].method).toBe('DELETE');
  });

  it('deleteTask throws ApiError with the status on failure', async () => {
    fetchMock.mockReturnValueOnce(
      Promise.resolve(new Response(null, { status: 404, statusText: 'Not Found' })),
    );

    await expect(api.deleteTask('abc')).rejects.toMatchObject({ name: 'ApiError', status: 404 });
  });

  it.each([
    ['getToday', () => api.getToday(), '/api/progress/today', undefined],
    ['getWeek', () => api.getWeek(), '/api/progress/week', undefined],
    ['markComplete', () => api.markComplete('t1'), '/api/progress/complete/t1', 'POST'],
    ['markIncomplete', () => api.markIncomplete('t1'), '/api/progress/incomplete/t1', 'POST'],
    ['getProfile', () => api.getProfile(), '/api/profile', undefined],
  ])('%s calls the right endpoint', async (_name, call, url, method) => {
    fetchMock.mockReturnValueOnce(respond({}));

    await call();

    expect(fetchMock.mock.calls[0][0]).toBe(url);
    expect(fetchMock.mock.calls[0][1]?.method).toBe(method);
  });

  it('updateProfile puts the profile body', async () => {
    fetchMock.mockReturnValueOnce(respond({ name: 'Mila', avatar: '🐻' }));

    await api.updateProfile({ name: 'Mila', avatar: '🐻' });

    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe('/api/profile');
    expect(init.method).toBe('PUT');
    expect(JSON.parse(init.body)).toEqual({ name: 'Mila', avatar: '🐻' });
  });

  it('validatePin posts the pin and returns validity', async () => {
    fetchMock.mockReturnValueOnce(respond({ valid: true }));

    await expect(api.validatePin('1234')).resolves.toEqual({ valid: true });

    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe('/api/auth/validate-pin');
    expect(JSON.parse(init.body)).toEqual({ pin: '1234' });
  });

  describe('error mapping', () => {
    it('uses the server error code and message', async () => {
      fetchMock.mockReturnValueOnce(
        respond({ error: { code: 'VALIDATION_ERROR', message: 'Bad label' } }, { status: 400 }),
      );

      const err = await api.getTasks().catch(e => e);

      expect(err).toBeInstanceOf(ApiError);
      expect(err).toMatchObject({ status: 400, code: 'VALIDATION_ERROR', message: 'Bad label' });
    });

    it('falls back to status text when the body is not JSON', async () => {
      fetchMock.mockReturnValueOnce(
        Promise.resolve(new Response('<html>', { status: 502, statusText: 'Bad Gateway' })),
      );

      const err = await api.getTasks().catch(e => e);

      expect(err.message).toBe('502 Bad Gateway');
      expect(err.code).toBeUndefined();
    });

    it('maps a service-worker offline response to the OFFLINE code', async () => {
      fetchMock.mockReturnValueOnce(respond({ offline: true }, { status: 503 }));

      const err = await api.getTasks().catch(e => e);

      expect(err.code).toBe('OFFLINE');
      expect(err.message).toBe('Offline mode - using cached data');
    });

    it('maps an aborted request to a TIMEOUT ApiError', async () => {
      vi.useFakeTimers();
      fetchMock.mockImplementationOnce(
        (_url: string, init: RequestInit) =>
          new Promise((_resolve, reject) => {
            init.signal!.addEventListener('abort', () => {
              const abort = new Error('aborted');
              abort.name = 'AbortError';
              reject(abort);
            });
          }),
      );

      const pending = api.getTasks().catch(e => e);
      await vi.advanceTimersByTimeAsync(30_000);

      expect(await pending).toMatchObject({ code: 'TIMEOUT', status: 0 });
    });

    it('rethrows network errors unchanged', async () => {
      fetchMock.mockRejectedValueOnce(new TypeError('Failed to fetch'));

      await expect(api.getTasks()).rejects.toThrow('Failed to fetch');
    });
  });
});
