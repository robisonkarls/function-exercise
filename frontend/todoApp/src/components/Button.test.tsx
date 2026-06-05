import { render, screen } from '@testing-library/react';
import { Button } from './Button';

describe('Button', () => {
    it('renders its children', () => {
        render(<Button>Click me</Button>);
        expect(screen.getByRole('button', { name: 'Click me' })).toBeInTheDocument();
    });

    it('applies Archive variant styles', () => {
        render(<Button variant="Archive">Archive</Button>);
        expect(screen.getByRole('button')).toHaveClass('bg-zinc-500');
    });
});
