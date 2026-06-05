import { render, screen, fireEvent } from '@testing-library/react';
import { TodoForm } from './TodoForm';

describe('TodoForm', () => {
    it('renders the input and Add Todo button', () => {
        render(<TodoForm onAdd={vi.fn()} />);
        expect(screen.getByPlaceholderText('What needs to be done?')).toBeInTheDocument();
        expect(screen.getByRole('button', { name: 'Add Todo' })).toBeInTheDocument();
    });

    it('shows an error when submitted with empty input', () => {
        render(<TodoForm onAdd={vi.fn()} />);
        fireEvent.submit(screen.getByRole('button', { name: 'Add Todo' }).closest('form')!);
        expect(screen.getByText('Title cannot be empty')).toBeInTheDocument();
    });
});
