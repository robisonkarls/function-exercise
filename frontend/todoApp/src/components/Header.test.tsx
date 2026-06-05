import { render, screen } from '@testing-library/react';
import Header from './Header';

describe('Header', () => {
    it('renders the Todo List heading', () => {
        render(<Header />);
        expect(screen.getByRole('heading', { name: 'Todo List' })).toBeInTheDocument();
    });

    it('renders the Send offer button', () => {
        render(<Header />);
        expect(screen.getByRole('button', { name: 'Send offer to Robison' })).toBeInTheDocument();
    });
});
