import { render, screen } from '@testing-library/react';
import { TodoList } from './TodoList';
import type { ITodo } from '../models/ITodos';

const noop = () => {};

const makeTodo = (overrides: Partial<ITodo> = {}): ITodo => ({
    id: 1,
    title: 'Buy milk',
    status: 'Pending',
    isArchived: false,
    createdAtUtc: '2024-01-01T00:00:00Z',
    updatedAtUtc: null,
    ...overrides,
});

describe('TodoList', () => {
    it('shows loading indicator when isLoading is true', () => {
        render(<TodoList todos={[]} isLoading={true} error={null} onComplete={noop} onArchive={noop} onUpdateTitle={noop} />);
        expect(screen.getByText('Loading...')).toBeInTheDocument();
    });

    it('shows empty message when todos array is empty', () => {
        render(<TodoList todos={[]} isLoading={false} error={null} onComplete={noop} onArchive={noop} onUpdateTitle={noop} />);
        expect(screen.getByText('No todos available')).toBeInTheDocument();
    });

    it('renders todo titles', () => {
        const todos = [makeTodo({ id: 1, title: 'Buy milk' }), makeTodo({ id: 2, title: 'Walk dog' })];
        render(<TodoList todos={todos} isLoading={false} error={null} onComplete={noop} onArchive={noop} onUpdateTitle={noop} />);
        expect(screen.getByText('Buy milk')).toBeInTheDocument();
        expect(screen.getByText('Walk dog')).toBeInTheDocument();
    });

    it('shows Complete for Pending and Archive for Completed', () => {
        const todos = [
            makeTodo({ id: 1, title: 'Pending task', status: 'Pending' }),
            makeTodo({ id: 2, title: 'Done task', status: 'Completed' }),
        ];
        render(<TodoList todos={todos} isLoading={false} error={null} onComplete={noop} onArchive={noop} onUpdateTitle={noop} />);
        expect(screen.getByRole('button', { name: 'Complete' })).toBeInTheDocument();
        expect(screen.getByRole('button', { name: 'Archive' })).toBeInTheDocument();
    });
});
