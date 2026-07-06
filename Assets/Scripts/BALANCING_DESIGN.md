# Game Balancing Design Guide

The usual approach is to combine a formal balance model, automated simulation, and targeted playtesting. You should not balance every stock combination manually.

For this game, I’d recommend this structure:

## 1. Define the economic targets

Establish expected values for each stage of a run:

- Expected income per spin
- Expected portfolio growth per day
- Rent progression
- Desired failure/success rate
- Expected value by rarity
- Acceptable variance
- Target run length

For example:

| Rarity | Expected immediate value | Variance | Intended role |
|---|---:|---:|---|
| Common | Low | Low | Reliable foundation |
| Rare | Medium | Medium | Synergy or scaling |
| Epic | High | High | Build-defining |
| Legendary | Potentially huge | Very high | Difficult conditions |

Rarity should not necessarily mean “strictly more money.” It can mean greater potential, stronger synergy, or less reliability.

## 2. Calculate effect value in a common unit

Convert every stock and effect into an estimated value per spin.

Examples:

- `+4 every 3 rounds` ≈ `+1.33 per round`
- `50% chance of +10, otherwise -2` = `0.5(10) + 0.5(-2) = +4`
- `+3 per matching adjacent stock` depends on expected adjacency frequency
- `+20 when at least four technology stocks exist` depends on the probability of satisfying that condition

This estimated value lets you compare mechanically different effects.

You should track at least:

- Immediate expected value
- Long-term expected value
- Variance
- Activation difficulty
- Scaling potential
- Synergy dependencies

## 3. Give effects a power budget

Each rarity receives an approximate budget. A stock’s base value and effects spend from that budget.

A simplified model could be:

```text
Stock power =
    base value
  + expected side-effect value
  + expected dividend value
  + expected growth value
```

Conditional effects should receive stronger numbers because they are less reliable. Effects with multiplicative or unlimited scaling require explicit caps, diminishing returns, or higher activation costs.

This does not produce final balance automatically, but it highlights obvious outliers before testing.

## 4. Build an automated simulator

This is the most valuable next step for your project.

The simulator should run thousands of games without animations or UI. It can randomly:

- Generate slot results
- Offer stocks using the actual rarity rules
- Choose stocks using different strategies
- Calculate effects and dividends
- Pay rent
- Add future items
- Record wins, failures, income, and stock performance

Useful simulated strategies include:

- Random choices
- Highest immediate value
- Same-sector synergy
- Growth-focused
- Dividend-focused
- High-risk/high-reward

Then record:

- Win rate
- Average money by day
- Failure day distribution
- Pick rate per stock
- Win rate when a stock is owned
- Average contribution per stock
- Strongest pairs and combinations
- Income variance
- Frequency of extreme runs

Run 10,000 simulated games instead of manually playing 10,000 games.

## 5. Track marginal contribution, not just total winnings

A stock appearing in winning runs does not necessarily make it strong. It may simply be commonly available.

Measure:

```text
Runs containing stock X
versus
similar runs without stock X
```

Also track how much value each effect directly generated. Your calculation system should eventually produce a breakdown such as:

```text
Stock A base value:          120
Stock A adjacency effect:     45
Stock A dividends:            30
Stock B multiplier benefit:  210
```

This attribution is essential for diagnosing why a combination is overpowered.

## 6. Test interactions systematically

With 40 stocks, pairwise testing already means 780 unique pairs. With items, exhaustive testing becomes impossible.

Use automated tests to cover:

- Every stock alone
- Every stock pair
- Stocks with their intended sector synergies
- Multiple effects modifying the same cell
- Multipliers combined with growth
- Negative values combined with multipliers
- Extreme adjacency layouts
- Maximum portfolio size
- Dividend timing boundaries

Prioritize interaction categories instead of every exact combination.

## 7. Separate balance data from mechanics

Your ScriptableObject and editor-table setup is already moving in the right direction.

Keep configurable data outside effect code:

- Base value
- Probability
- Failure penalty
- Interval
- Multiplier
- Threshold
- Caps
- Rarity weights
- Unlock day

This allows balance changes without modifying mechanics. Exported JSON/CSV also makes it easier to analyze values in a spreadsheet.

## 8. Use guardrails for dangerous mechanics

The mechanics most likely to break the economy are:

- Multiplication
- Effects that trigger other effects
- Unlimited permanent growth
- Bonuses per instance without caps
- Chance effects with enormous payouts
- Items affecting every stock
- Effects that scale both frequency and value

Define rules early, such as:

- Multipliers do not multiply other multipliers.
- An effect triggers at most once per cell or spin.
- Permanent growth has a cap.
- Global effects use diminishing returns.
- Trigger chains cannot recurse.
- The order of calculations is explicitly fixed.

Your existing base → side effects → portfolio payouts phases are a useful foundation for this.

## 9. Use playtesting for questions simulation cannot answer

Simulation can tell you that a stock is strong. It cannot reliably tell you:

- Whether the effect is understandable
- Whether a payout feels satisfying
- Whether a build is enjoyable
- Whether choices feel meaningful
- Whether randomness feels fair
- Whether the player understands why money changed

Automated simulation narrows the problem. Human playtesting evaluates experience and validates the resulting adjustments.

The practical next step for this project would be a headless balance simulator using the real stock definitions and calculation rules. Before building it, I would introduce structured calculation reporting so every base value, effect, and dividend records its source and contribution. That reporting would support both the simulator and an in-game debug breakdown.
