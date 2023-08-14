(**
<!-- header: 'Computation Expression | Option | Result | **Monad** | Result ex. | Async | Overloaded Binds | Task' -->

# CE and Monad

---

# CE and Monad

Is Computation Expression a Monad?

<!-- Monády, o nichž zde bude řeč, nejsou nic jiného než jednoduché substance, z nichž se dohromady skládají věci   -->

![](comp-expr/img/quote-the-monad-of-which-we-shall-speak-here-is-nothing-but-a-simple-substance-which-enters-gottfried-leibniz-76-55-34-2688366744.png)

---

## What Monad?
- Category theory
- Functional programming
- <i>Linear algebra (we skip this one)</i>
- Philosophy

---

## Is Computation Expression a (Category theory) Monad?

![](comp-expr/img/44b0bd758f8ee5c81362923f0d5c8e017c9ddf623925e60c29a4c015b89fbb45-3127675396.png)

---

- No

- (Category theory) Monad is an abstract math term, that's not really useful in the context of programming.

<!-- Monoid on top of functions -->

- ![](comp-expr/img/monad-ct.png)

- Very roughly: Monad is a monoid on top of functions (function = code block).


---

## Is Computation Expression a (FP) Monad?

- Yes 
- and No

---

### ✅ Monad laws in FP

> A monad can be created by defining a type constructor M and two operations:
> `return :: a -> M a` (often also called unit), which receives a value of type `a` and wraps it into a monadic value of type `m a`, and
> `bind :: (M a) -> (a -> M b) -> (M b)` (typically represented as `>>=`), which receives a function `f` over type `a` and can transform monadic values `m a` applying `f` to the unwrapped value `a`, returning a monadic value `M b`.

**Looks like exactly what we have in Computation Expression.**

---

## ❌

- There is nothing as "Monad" type `M` in F# - we can't use `Bind` of generic *CE*.
- It's not possible due to lack of <i>higher kinded types</i>.
- <i>Higher kinded types</i> are not supported in F# by design, because it leads to type over-engineering.
- ![](comp-expr/img/Typeclassopedia-diagram.png)

---

## Abstraction and application

- **Monad as Category Theory abstraction**

- ⬇ application - replace transformations with functions

- **Monad in FP - `Bind` and `Return` functions**

- ⬇ application - fixing "monad" type

- **FP Monad in F# - Computation Expression**

---

## Is this Monad thing important for F#?

- CE is a sort of application of application of Monad.
- Understanding Monad is not necessary to understand CE.
- It can be useful to know connection between CE and Monad.

*)

(** --- *)


